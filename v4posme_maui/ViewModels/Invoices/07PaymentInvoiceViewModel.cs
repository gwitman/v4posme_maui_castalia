using CommunityToolkit.Maui.Core;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.Invoices;
using Unity;
using CommunityToolkit.Maui.Views;

namespace v4posme_maui.ViewModels.Invoices;

public class PaymentInvoiceViewModel : BaseViewModel
{
    private readonly HelperCore _helper;
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryItems _repositoryItems;
    private readonly IRepositoryParameters _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
    private readonly IRepositoryTbCustomer _repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
    private readonly RestApiAppMobileApi _restApiDownload = new RestApiAppMobileApi();

    public PaymentInvoiceViewModel()
    {
        _helper                                 = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _repositoryTbTransactionMasterDetail    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        _repositoryTbTransactionMaster          = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryItems                        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        Title                                   = "Pago 6/6";
        SelectionEfectivoCommand                = new Command(OnSelectionEfectivoCommand);
        SelectionRegistrarCommand               = new Command(OnSelectionRegistrarCommand);
        SelectionDebitoCommand                  = new Command(OnSelectionDebitoCommand);
        SelectionCreditoCommand                 = new Command(OnSelectionCreditoCommand);
        SelectionMonederoCommand                = new Command(OnSelectionMonederoCommand);
        SelectionChequeCommand                  = new Command(OnSelectionChequeCommand);
        SelectionOtrosCommand                   = new Command(OnSelectionOtrosCommand);
        AplicarPagoCommand                      = new Command(OnAplicarPagoCommand, OnValidatePago);
        ClearMontoCommand                       = new Command(OnClearMontoCommand);
        PagarSeleccion                          = "Pagar con Selección";
        PropertyChanged += (_, _) => AplicarPagoCommand.ChangeCanExecute();
    }

    private bool Validate()
    {
        return decimal.Compare(Monto, decimal.Zero) <= 0 || !ValidarSeleccionPago();
    }

    private void OnClearMontoCommand(object obj)
    {
        Monto = decimal.Zero;
        Cambio = decimal.Zero;
    }

    private bool OnValidatePago()
    {
        return !Validate();
    }

    private bool ValidarSeleccionPago()
    {
        return ChkCheque || ChkCredito || ChkDebito || ChkEfectivo || ChkMonedero || ChkOtros || ChkRegistrar;
    }

    private const string Screen = "PaymentInvoice(07)";

    private async void OnAplicarPagoCommand()
    {
        HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "inicio");
        if (!ValidarSeleccionPago())
        {
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "no se seleccionó tipo de pago", "Warning");
            ShowToast(Mensajes.MensajeSeleccionarTipoPago, ToastDuration.Long, 12);
            return;
        }

        try
        {
            IsBusy          = true;
            var dtoInvoice  = VariablesGlobales.DtoInvoice;

            // Rastrear estado del DTO antes de usarlo para detectar la referencia null exacta
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "validando estado del DtoInvoice y dependencias globales");
            HelperLogs.TraceValue(Screen, "VariablesGlobales.DtoInvoice", dtoInvoice);
            HelperLogs.TraceValue(Screen, "VariablesGlobales.User", VariablesGlobales.User);
            HelperLogs.TraceValue(Screen, "DtoInvoice.CustomerResponse", dtoInvoice?.CustomerResponse);
            HelperLogs.TraceValue(Screen, "DtoInvoice.TipoDocumento", dtoInvoice?.TipoDocumento);
            HelperLogs.TraceValue(Screen, "DtoInvoice.Currency", dtoInvoice?.Currency);
            HelperLogs.TraceValue(Screen, "DtoInvoice.PeriodPay", dtoInvoice?.PeriodPay);
            HelperLogs.TraceValue(Screen, "DtoInvoice.Mesa", dtoInvoice?.Mesa);
            HelperLogs.TraceValue(Screen, "DtoInvoice.Items", dtoInvoice?.Items);
            HelperLogs.TraceValue(Screen, "DtoInvoice.TipoDocumento.Key", dtoInvoice?.TipoDocumento?.Key);
            HelperLogs.TraceValue(Screen, "DtoInvoice.CustomerResponse.EntityId", dtoInvoice?.CustomerResponse?.EntityId);
            HelperLogs.TraceValue(Screen, "DtoInvoice.CustomerResponse.Identification", dtoInvoice?.CustomerResponse?.Identification);
            HelperLogs.TraceValue(Screen, "DtoInvoice.Currency.Key", dtoInvoice?.Currency?.Key);
            HelperLogs.TraceValue(Screen, "DtoInvoice.PeriodPay.Key", dtoInvoice?.PeriodPay?.Key);
            HelperLogs.TraceValue(Screen, "DtoInvoice.Mesa.Key", dtoInvoice?.Mesa?.Key);

            // Validaciones explicitas para evitar NullReferenceException silencioso
            if (dtoInvoice is null)
                throw new InvalidOperationException("VariablesGlobales.DtoInvoice es null. No hay factura activa en memoria.");
            if (VariablesGlobales.User is null)
                throw new InvalidOperationException("VariablesGlobales.User es null. No hay sesión de usuario activa.");
            if (dtoInvoice.CustomerResponse is null)
                throw new InvalidOperationException("DtoInvoice.CustomerResponse es null. No se seleccionó cliente.");
            if (dtoInvoice.TipoDocumento is null)
                throw new InvalidOperationException("DtoInvoice.TipoDocumento es null. No se definió el tipo de documento.");
            if (dtoInvoice.Currency is null)
                throw new InvalidOperationException("DtoInvoice.Currency es null. No se definió la moneda.");
            if (dtoInvoice.Items is null || dtoInvoice.Items.Count == 0)
                throw new InvalidOperationException("DtoInvoice.Items está vacío. No hay productos en la factura.");

            // En facturas de contado la pantalla de crédito (3/6) se omite y PeriodPay
            // queda null. Se aplica un valor por defecto en lugar de reventar con NullReference.
            if (dtoInvoice.PeriodPay is null)
            {
                HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "PeriodPay era null (factura de contado). Aplicando valor por defecto Mensual", "Warning");
                dtoInvoice.PeriodPay = new DtoCatalogItem((int)TypePeriodPay.Mensual, "Mensual", "M");
            }

            // Mesa puede quedar null si no se usa el módulo de restaurante. Se aplica un valor neutro.
            if (dtoInvoice.Mesa is null)
            {
                HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "Mesa era null. Aplicando valor por defecto (0)", "Warning");
                dtoInvoice.Mesa = new DtoCatalogItem(0, "Seleccione", "Seleccione");
            }

            var codigo = "";
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"obteniendo código de factura (TransactionMasterId={dtoInvoice.TransactionMasterId})");
            if (dtoInvoice.TransactionMasterId <= 0 )
                codigo = await _helper.GetCodigoFactura();
            else
                codigo = dtoInvoice.Codigo;
            HelperLogs.TraceValue(Screen, "codigo", codigo);

            //Eliminar el registro en caso de ser edicion
            if(dtoInvoice.TransactionMasterId > 0)
            {
                HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"edición: eliminando factura anterior (TransactionMasterId={dtoInvoice.TransactionMasterId})");
                var invoiceOld          = await _repositoryTbTransactionMaster.PosMeFindByTransactionId(dtoInvoice.TransactionMasterId);
                HelperLogs.TraceValue(Screen, "invoiceOld", invoiceOld);
                var invoiceDetailOld    = await _repositoryTbTransactionMasterDetail.PosMeItemByTransactionId(dtoInvoice.TransactionMasterId);
                HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"detalles anteriores a eliminar: {invoiceDetailOld?.Count ?? 0}");
                if (invoiceDetailOld is not null)
                {
                    foreach (var invoiceDetailOld_i in invoiceDetailOld)
                    {
                        await _repositoryTbTransactionMasterDetail.PosMeDelete(invoiceDetailOld_i);
                    }
                }
                if (invoiceOld is not null)
                    await _repositoryTbTransactionMaster.PosMeDelete(invoiceOld);
            }

            VariablesGlobales.DtoInvoice.Codigo         = codigo;
            VariablesGlobales.DtoInvoice.Monto          = Monto;
            VariablesGlobales.DtoInvoice.Cambio         = Cambio;
            VariablesGlobales.DtoInvoice.TransactionOn  = DateTime.Now;

            //Obtener el estado de la factura
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "validando permiso para determinar estado de la factura");
            int statusID            = 0;
            bool permission = await _helper.GetPermission(TypeMenuElementID.core_billing_invoice_type_restaurant, TypePermission.Updated, TypeImpact.All);
            statusID = !permission ? (int)TypeStatusBilling.Apply : (int)TypeStatusBilling.Register;
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"permission={permission}, statusID={statusID}");

            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "construyendo transacción maestra");
            var transactionMaster   = new TbTransactionMaster
            {
                TransactionId       = TypeTransaction.TransactionInvoiceBilling,
                Amount              = Monto,
                TransactionOn       = DateTime.Now,
                TransactionCausalId = (TypeTransactionCausal)dtoInvoice.TipoDocumento!.Key,
                TypePaymentId       = TypePayment,
                Comment             = dtoInvoice.Comentarios,
                Discount            = decimal.Zero,
                Taxi1               = decimal.Zero,
                ExchangeRate        = decimal.Zero, //definir
                EntityId            = dtoInvoice.CustomerResponse!.EntityId,
                EntitySecondaryId   = VariablesGlobales.User!.UserId.ToString(),
                TransactionNumber   = codigo,
                CurrencyId          = (TypeCurrency)dtoInvoice.Currency!.Key,
                CustomerCreditLineId = dtoInvoice.CustomerResponse.CustomerCreditLineId,
                CustomerIdentification = dtoInvoice.CustomerResponse.Identification!,
                Plazo               = dtoInvoice.Plazo,
                NextVisit           = dtoInvoice.NextVisit,
                FixedExpenses       = dtoInvoice.FixedExpenses,
                PeriodPay           = (TypePeriodPay)dtoInvoice.PeriodPay!.Key,
                StatusID            = statusID,
                MesaID              = dtoInvoice.Mesa!.Key,
                ReferenceClientName = dtoInvoice.ReferenceClientName,
                MesaName            = dtoInvoice.Mesa!.Name,
                RegisterLocal       = 1
            };

            transactionMaster.SubAmount = dtoInvoice.Balance + dtoInvoice.Items.Sum(P => P.MontoDescuento);
            transactionMaster.Amount    = dtoInvoice.Balance + dtoInvoice.Items.Sum(P => P.MontoDescuento);
            transactionMaster.Discount  = dtoInvoice.Items.Sum(P => P.MontoDescuento);
            HelperLogs.DumpObject(Screen, "transactionMaster", transactionMaster);
            var listMasterDetail        = new List<TbTransactionMasterDetail>();
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"insertando transacción maestra (Amount={transactionMaster.Amount}, codigo={codigo})");
            await _repositoryTbTransactionMaster.PosMeInsert(transactionMaster);
            var transactionMasterId     = transactionMaster.TransactionMasterId;
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"transacción maestra insertada (TransactionMasterId={transactionMasterId})");

            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"construyendo detalle de {dtoInvoice.Items.Count} items");
            foreach (var item in dtoInvoice.Items)
            {
                HelperLogs.TraceValue(Screen, "item", item);
                HelperLogs.TraceValue(Screen, "item.ItemNumber", item?.ItemNumber);
                if (item is null)
                    throw new InvalidOperationException("Un item del detalle de la factura es null.");
                if (string.IsNullOrEmpty(item.ItemNumber))
                    throw new InvalidOperationException($"item.ItemNumber es null/vacío (ItemId={item.ItemId}).");

                var findPrecioOriginal = await _repositoryItems.PosMeFindByItemNumber(item.ItemNumber!);
                HelperLogs.TraceValue(Screen, $"findPrecioOriginal({item.ItemNumber})", findPrecioOriginal);
                if (findPrecioOriginal is null)
                    throw new InvalidOperationException($"No se encontró el producto con ItemNumber={item.ItemNumber} para obtener el precio original.");

                var detail = new TbTransactionMasterDetail
                {
                    Quantity            = item.Quantity,
                    UnitaryCost         = findPrecioOriginal.PrecioPublico,
                    UnitaryPrice        = item.PrecioPublico,
                    TransactionMasterId = transactionMasterId, /**/
                    SubAmount           = item.Importe,
                    Discount            = item.MontoDescuento,
                    Tax1                = decimal.Zero,
                    Componentid         = (int)TypeComponent.Itme,
                    ComponentItemId     = item.ItemId,
                    ItemBarCode         = item.BarCode,
                    ReferenciaProducto  = item.Referencia,
                    RegisterLocal       = 1
                };
                detail.Amount = detail.SubAmount;
                HelperLogs.DumpObject(Screen, $"detail(ItemNumber={item.ItemNumber})", detail);
                listMasterDetail.Add(detail);
            }

            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"insertando {listMasterDetail.Count} detalles de la factura");
            await _repositoryTbTransactionMasterDetail.PosMeInsertAll(listMasterDetail);
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "actualizando contador (PlusCounter)");
            await _helper.PlusCounter();
            VariablesGlobales.EnableBackButton              = false;
            VariablesGlobales.DtoInvoice.TipoPayment        = TypePayment;
            VariablesGlobales.DtoInvoice.TransactionMaster  = transactionMaster;

            //Pasar a otra ventana
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", "proceso finalizado con éxito, navegando a impresión");
            if (Navigation is null)
                throw new InvalidOperationException("Navigation es null. No se puede navegar a la pantalla de impresión.");
            await Navigation.PushAsync(new PrinterInvoicePage());
            IsBusy = false;
        }
        catch (Exception ex)
        {
            HelperLogs.Trace(Screen, "OnAplicarPagoCommand", $"EXCEPCIÓN: {ex.GetType().Name} - {ex.Message}", "Error");
            HelperLogs.Log(ex);
            ShowMensajePopUp(ex.Message);
            IsBusy = false;
        }
    }

    private void OnSelectionOtrosCommand()
    {
        ChangedChecked(false, false, false, false, false, true,false);
        PagarSeleccion = "Pagar con Otros";
    }

    private void OnSelectionChequeCommand()
    {
        ChangedChecked(false, false, false, false, true, false,false);
        PagarSeleccion = "Pagar con Cheque";
    }

    private void OnSelectionMonederoCommand()
    {
        ChangedChecked(false, false, false, true, false, false,false);
        PagarSeleccion = "Pagar con Monedero";
    }

    private void OnSelectionCreditoCommand()
    {
        ChangedChecked(false, false, true, false, false, false,false);
        PagarSeleccion = "Pagar con Credito";
    }

    private void OnSelectionDebitoCommand()
    {
        ChangedChecked(false, true, false, false, false, false  ,false);
        PagarSeleccion = "Pagar con Debito";
    }

    private void OnSelectionRegistrarCommand()
    {
        ChangedChecked(false, false, false, false, false, false, true);
        PagarSeleccion = "Registrar";
    }
    private void OnSelectionEfectivoCommand()
    {
        ChangedChecked(true, false, false, false, false, false,false);
        PagarSeleccion = "Pagar con Efectivo";
    }

    public string Moneda => VariablesGlobales.DtoInvoice.Currency!.Simbolo;
    public decimal Balance => VariablesGlobales.DtoInvoice.Balance;

    private bool _chkEfectivo;
    

    public bool ChkEfectivo
    {
        get => _chkEfectivo;
        set => SetProperty(ref _chkEfectivo, value);
    }

    private bool _chkRegistrar;
    public bool ChkRegistrar
    {
        get => _chkRegistrar;
        set => SetProperty(ref _chkRegistrar, value);
    }

    private bool _chkCredito;

    public bool ChkCredito
    {
        get => _chkCredito;
        set => SetProperty(ref _chkCredito, value);
    }

    private bool _chkDebito;

    public bool ChkDebito
    {
        get => _chkDebito;
        set => SetProperty(ref _chkDebito, value);
    }

    private bool _chkCheque;

    public bool ChkCheque
    {
        get => _chkCheque;
        set => SetProperty(ref _chkCheque, value);
    }

    private bool _chkMonedero;

    public bool ChkMonedero
    {
        get => _chkMonedero;
        set => SetProperty(ref _chkMonedero, value);
    }

    private bool _chkOtros;

    public bool ChkOtros
    {
        get => _chkOtros;
        set => SetProperty(ref _chkOtros, value);
    }

    private decimal _monto = VariablesGlobales.DtoInvoice.Balance;

    public decimal Monto
    {
        get => _monto;
        set
        {
            SetProperty(ref _monto, value);
            _cambio = decimal.Subtract(value, Balance);
            OnPropertyChanged(nameof(Cambio));
        }
    }

    private decimal _cambio;

    public decimal Cambio
    {
        get => _cambio;
        set => SetProperty(ref _cambio, value);
    }

    public Command SelectionEfectivoCommand { get; }
    public Command SelectionRegistrarCommand { get; }
    public Command SelectionDebitoCommand { get; }
    public Command SelectionCreditoCommand { get; }
    public Command SelectionMonederoCommand { get; }
    public Command SelectionChequeCommand { get; }
    public Command SelectionOtrosCommand { get; }
    private TypePayment TypePayment { get; set; }
    private string? _pagarSeleccion;

    public string? PagarSeleccion
    {
        get => _pagarSeleccion;
        set => SetProperty(ref _pagarSeleccion, value);
    }

    public Command AplicarPagoCommand { get; }
    public Command ClearMontoCommand { get; }

    private void ChangedChecked(bool efectivo, bool debito, bool credito, bool monedero, bool cheque, bool otros,bool registrar)
    {
        ChkEfectivo     = efectivo;
        ChkCredito      = credito;
        ChkDebito       = debito;
        ChkCheque       = cheque;
        ChkMonedero     = monedero;
        ChkOtros        = otros;
        ChkRegistrar    = registrar;

        if(registrar)
        {
            TypePayment = TypePayment.Registrar;
        }

        if (efectivo)
        {
            TypePayment = TypePayment.Efectivo;
        }

        if (credito)
        {
            TypePayment = TypePayment.TarjetaCredito;
            Shareurl();
        }

        if (debito)
        {
            TypePayment = TypePayment.TarjetaDebito;
            Shareurl();
        }

        if (monedero)
        {
            TypePayment = TypePayment.Monedero;
        }

        if (cheque)
        {
            TypePayment = TypePayment.Cheque;
        }

        if (otros)
        {
            TypePayment = TypePayment.Otros;
        }
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        IsBusy = false;
    }

    private async void Shareurl()
    {
        if (decimal.Compare(Monto, decimal.Zero)<=0)
        {
            ShowToast(Mensajes.MensajeMontoMenorIgualCero,ToastDuration.Long,12);
            return;
        }
        IsBusy                  = true;
        var uid                 = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_USUARIO_COMMERCECLIENT");
        var awk                 = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_CLAVE_COMMERCECLIENTE");        
        var urlCommerce         = "http://posme.net";
        var operationRequest    = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_OPERTATIONID_CONNECT");
        var operationExec       = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_OPERTATIONID_EXEC");
        var realizarPago        = new RestApiPagadito();
        var tm                  = new TbTransactionMaster()
        {
            Amount      = Monto,
            CurrencyId  = (TypeCurrency)VariablesGlobales.DtoInvoice.Currency!.Key
        };
        HelperLogs.DumpObject(Screen, "tm(Pagadito)", tm);
        var response    = await realizarPago.GenerarUrl(uid!.Value!, awk!.Value!,urlCommerce,
            operationRequest!.Value!,operationExec!.Value!,VariablesGlobales.DtoInvoice.Items.ToList(), tm);
        IsBusy          = false;
        if (response is not null)
        {
            if (response.Value != "")
            { 
                await Share.RequestAsync(new ShareTextRequest
                {
                    Uri     = response.Value,
                    Title   = "Realizar pago de compras"
                });
            }
            else
            {
                ShowToast(realizarPago.Mensaje, ToastDuration.Long, 12);
            }
        }
        else
        {
            ShowToast(realizarPago.Mensaje, ToastDuration.Long, 12);
        }
    }
}