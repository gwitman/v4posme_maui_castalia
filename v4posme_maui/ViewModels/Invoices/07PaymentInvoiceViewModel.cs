using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.Invoices;
using Unity;

namespace v4posme_maui.ViewModels.Invoices;

public class PaymentInvoiceViewModel : BaseViewModel
{
    private readonly HelperCore _helper;
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryItems _repositoryItems;
    private readonly IRepositoryParameters _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();

    public PaymentInvoiceViewModel()
    {
        _helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _repositoryTbTransactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        Title = "Pago 6/6";
        SelectionEfectivoCommand = new Command(OnSelectionEfectivoCommand);
        SelectionDebitoCommand = new Command(OnSelectionDebitoCommand);
        SelectionCreditoCommand = new Command(OnSelectionCreditoCommand);
        SelectionMonederoCommand = new Command(OnSelectionMonederoCommand);
        SelectionChequeCommand = new Command(OnSelectionChequeCommand);
        SelectionOtrosCommand = new Command(OnSelectionOtrosCommand);
        AplicarPagoCommand = new Command(OnAplicarPagoCommand, OnValidatePago);
        ClearMontoCommand = new Command(OnClearMontoCommand);
        PagarSeleccion = "Pagar con Selección";
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
        return ChkCheque || ChkCredito || ChkDebito || ChkEfectivo || ChkMonedero || ChkOtros;
    }

    private async void OnAplicarPagoCommand()
    {
        if (!ValidarSeleccionPago())
        {
            ShowToast(Mensajes.MensajeSeleccionarTipoPago, ToastDuration.Long, 12);
            return;
        }

        IsBusy          = true;
        var dtoInvoice  = VariablesGlobales.DtoInvoice;
        var codigo      = await _helper.GetCodigoFactura();
        VariablesGlobales.DtoInvoice.Codigo         = codigo;
        VariablesGlobales.DtoInvoice.Monto          = Monto;
        VariablesGlobales.DtoInvoice.Cambio         = Cambio;
        VariablesGlobales.DtoInvoice.TransactionOn  = DateTime.Now;
        var transactionMaster = new TbTransactionMaster
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
            PeriodPay           = (TypePeriodPay)dtoInvoice.PeriodPay!.Key
        };
        transactionMaster.SubAmount = dtoInvoice.Balance - transactionMaster.Discount + transactionMaster.Taxi1;

        var listMasterDetail    = new List<TbTransactionMasterDetail>();
        await _repositoryTbTransactionMaster.PosMeInsert(transactionMaster);
        var transactionMasterId = transactionMaster.TransactionMasterId;

        foreach (var item in dtoInvoice.Items)
        {
            var findPrecioOriginal = await _repositoryItems.PosMeFindByItemNumber(item.ItemNumber!);
            var detail = new TbTransactionMasterDetail
            {
                Quantity            = item.Quantity,
                UnitaryCost         = findPrecioOriginal.PrecioPublico,
                UnitaryPrice        = item.PrecioPublico,
                TransactionMasterId = transactionMasterId, /**/
                SubAmount           = item.Importe,
                Discount            = decimal.Zero,
                Tax1                = decimal.Zero,
                Componentid         = (int)TypeComponent.Itme,
                ComponentItemId     = item.ItemId,
                ItemBarCode         = item.BarCode
            };
            detail.Amount = detail.SubAmount - detail.Discount + detail.Tax1;
            listMasterDetail.Add(detail);
        }

        await _repositoryTbTransactionMasterDetail.PosMeInsertAll(listMasterDetail);
        await _helper.PlusCounter();
        VariablesGlobales.EnableBackButton              = false;
        VariablesGlobales.DtoInvoice.TipoPayment        = TypePayment;
        VariablesGlobales.DtoInvoice.TransactionMaster  = transactionMaster;
        await Navigation!.PushAsync(new PrinterInvoicePage());
        IsBusy = false;
    }

    private void OnSelectionOtrosCommand()
    {
        ChangedChecked(false, false, false, false, false, true);
        PagarSeleccion = "Pagar con Otros";
    }

    private void OnSelectionChequeCommand()
    {
        ChangedChecked(false, false, false, false, true, false);
        PagarSeleccion = "Pagar con Cheque";
    }

    private void OnSelectionMonederoCommand()
    {
        ChangedChecked(false, false, false, true, false, false);
        PagarSeleccion = "Pagar con Monedero";
    }

    private void OnSelectionCreditoCommand()
    {
        ChangedChecked(false, false, true, false, false, false);
        PagarSeleccion = "Pagar con Credito";
    }

    private void OnSelectionDebitoCommand()
    {
        ChangedChecked(false, true, false, false, false, false);
        PagarSeleccion = "Pagar con Debito";
    }

    private void OnSelectionEfectivoCommand()
    {
        ChangedChecked(true, false, false, false, false, false);
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

    private void ChangedChecked(bool efectivo, bool debito, bool credito, bool monedero, bool cheque, bool otros)
    {
        ChkEfectivo = efectivo;
        ChkCredito = credito;
        ChkDebito = debito;
        ChkCheque = cheque;
        ChkMonedero = monedero;
        ChkOtros = otros;
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