using System.Diagnostics;
using System.Windows.Input;
using DevExpress.Maui.Scheduler;
using Plugin.BLE;
using SkiaSharp;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.More.ProductosVendidos;

public class ProductosVendidosViewModel : BaseViewModel
{
    private readonly IRepositoryItems _repositoryItems;
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryParameters _repositoryParameters;
    private readonly IRepositoryTbTransactionMasterDetail _transactionMasterDetail;

    public ProductosVendidosViewModel()
    {
        _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _transactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        _items = new();
        PrintCommand = new Command(OnPrintCommand);
        LoadDataCommand = new Command(OnLoadDataCommand);


    }

    public ICommand PrintCommand { get; private set; }
    public ICommand LoadDataCommand { get; }

    private DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> _items;
    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Items
    {
        get => _items;
        set => SetProperty(ref _items, value);
    }

    private Api_AppMobileApi_GetDataDownloadParametersResponse? _companyTelefono;

    public Api_AppMobileApi_GetDataDownloadParametersResponse? CompanyTelefono
    {
        get => _companyTelefono;
        private set => SetProperty(ref _companyTelefono, value);
    }

    private Api_AppMobileApi_GetDataDownloadParametersResponse? _companyRuc;

    public Api_AppMobileApi_GetDataDownloadParametersResponse? CompanyRuc
    {
        get => _companyRuc;
        private set => SetProperty(ref _companyRuc, value);
    }

    private DateTime _fechaInical;
    private DateTime _fechaFinal;

    public DateTime FechaInical
    {
        get => _fechaInical;
        set
        {
            SetProperty(ref _fechaInical, value);
        }
    }

    public DateTime FechaFinal
    {
        get => _fechaFinal;
        set
        {
            /*_fechaFinal = value;*/
            SetProperty(ref _fechaFinal, value);
        }
    }


    private async void OnLoadDataCommand()
    {
        LoadData();
    }
    private async void OnPrintCommand()
    {
        try
        {
            var parametroPrinter = await _parameterSystem.PosMeFindPrinter();
            var logo = await _parameterSystem.PosMeFindLogo();
            if (string.IsNullOrWhiteSpace(parametroPrinter.Value))
            {
                return;
            }

            var printer = new Printer(parametroPrinter.Value);
            if (!CrossBluetoothLE.Current.IsOn)
            {
                ShowMensajePopUp(Mensajes.MensajeBluetoothState);
                return;
            }

            IsBusy = true;
            if (!string.IsNullOrWhiteSpace(logo.Value))
            {
                var readImage = Convert.FromBase64String(logo.Value!);
                printer.AlignRight();
                printer.Image(SKBitmap.Decode(readImage));
            }

            var user = VariablesGlobales.User;
            if (user is null)
            {
                ShowMensajePopUp(Mensajes.UsuarioNoExiste);
                return;
            }
            if (Company is null)
            {
                ShowMensajePopUp(Mensajes.CompanyNoExiste);
                return;
            }


            if (FechaInical == DateTime.MinValue)
            {
                var hoy = DateTime.Now;
                FechaInical = new DateTime(hoy.Year, hoy.Month, hoy.Day);
            }

            if (FechaFinal == DateTime.MinValue)
            {
                FechaFinal = DateTime.Now;
            }

            DateTime startOn    = FechaInical;
            DateTime endOn      = new DateTime(FechaFinal.Year, FechaFinal.Month, FechaFinal.Day).AddDays(1).AddSeconds(-1);

            printer.AlignCenter();
            printer.BoldMode(Company.Name!);
            printer.BoldMode($"RUC: {CompanyRuc!.Value}");
            printer.BoldMode("Productos vendidos");
            printer.BoldMode(DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
            printer.NewLine();
            printer.AlignLeft();
            printer.Append($"El usuario {user.Nickname} a generado el reporte de productos vendidos");
            printer.NewLine();

            decimal amountTotal = 0;
            if (Items.Count > 0)
            {



                printer.Append("Detalle de productos");
                printer.Append("Descripción     Barra      Qt");
                printer.NewLine();
                foreach (var item in Items)
                {
                    //if (item.BarCode == "22")
                    //{
                    //    string debug = "debug point";
                    //}


                    //Obtener la cantidad facturadas por prodcuto
                    decimal quantityInvoice      = 0;
                    decimal amountInvoice        = 0;
                    var objListTransactionDetail = await _transactionMasterDetail.PosMeByTransactionIDAndItemID_BetweenDate((int)TypeTransaction.TransactionInvoiceBilling, item.ItemId, startOn, endOn);
                    if (objListTransactionDetail is null)
                    {
                        quantityInvoice = 0;
                        amountInvoice   = 0;
                    }
                    else
                    {
                        quantityInvoice = Convert.ToDecimal(objListTransactionDetail.Sum(p => p.Quantity));
                        amountInvoice   = Convert.ToDecimal(objListTransactionDetail.Sum(p => p.Amount));
                    }

                    //Obtener las cantidades por productos
                    decimal quantityByItem = 0;
                    quantityByItem  = (item.Quantity + item.CantidadEntradas) - (item.CantidadSalidas + quantityInvoice);
                    quantityByItem  = quantityInvoice;
                    amountTotal     = amountTotal + amountInvoice;
                    

                    var barCode = item.BarCode;
                    barCode = barCode.Split(",")[0];

                    if (barCode.Length < 12)
                        barCode = barCode.PadLeft(12, '0'); // Rellena con ceros a la izquierda
                    else
                        barCode = barCode.Substring(0, 12); // Corta a 12 caracteres

                    if (quantityInvoice != 0)
                    {
                        printer.Append($"{item.Name}");
                        printer.Append($"{barCode}               {quantityInvoice:N2}");
                        printer.Append($"{barCode}               {amountInvoice:N2}");
                        printer.Append("..............................");
                    }

                }
            }
            printer.NewLine();
            printer.Append($"TOTAL #  ARTICULOS: {Items.Count}");
            printer.Append($"TOTAL C$ ARTICULOS: {amountTotal:N2}");
            printer.NewLine();
            printer.AlignCenter();
            printer.Append(CompanyTelefono!.Value!);
            printer.Append(Company.Address!);
            printer.FullPaperCut();
            printer.Print();
            if (printer.Device is null)
            {
                ShowMensajePopUp(Mensajes.MensajeDispositivoNoConectado);
            }

            IsBusy = false;
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
    }


    public async void LoadData()
    {
        try
        {
            var data            = await _repositoryItems.PosMeFindAll();
            var itemsTemporal   = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>(data);
            Items               = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>();

            if (FechaInical == DateTime.MinValue)
            {
                var hoy         = DateTime.Now;
                FechaInical     = new DateTime(hoy.Year, hoy.Month, hoy.Day);
            }

            if (FechaFinal == DateTime.MinValue)
            {
                FechaFinal = DateTime.Now;
            }


            DateTime startOn    = FechaInical;
            DateTime endOn      = new DateTime(FechaFinal.Year, FechaFinal.Month, FechaFinal.Day).AddDays(1).AddSeconds(-1);



            //Calcular los prodcutos cantidade finales
            if (itemsTemporal is not null)
            {
                foreach (var item in itemsTemporal)
                {
                    decimal quantityInvoice = 0;
                    var objListTransactionDetail = await _transactionMasterDetail.PosMeByTransactionIDAndItemID_BetweenDate((int)TypeTransaction.TransactionInvoiceBilling, item.ItemId,startOn,endOn);
                    if (objListTransactionDetail is null)
                        quantityInvoice = 0;
                    else
                        quantityInvoice = Convert.ToDecimal(objListTransactionDetail.Sum(p => p.Quantity));

                    item.Quantity = (item.Quantity + item.CantidadEntradas) - (item.CantidadSalidas + quantityInvoice);
                    item.Quantity = quantityInvoice;

                    if (item.Quantity > 0)
                    {
                        Items.Add(item);
                    }

                }
            }

            CompanyTelefono = await _repositoryParameters.PosMeFindByKey("CORE_PHONE");
            CompanyRuc      = await _repositoryParameters.PosMeFindByKey("CORE_COMPANY_IDENTIFIER");
            Company         = VariablesGlobales.TbCompany;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            ShowMensajePopUp(e.Message);
        }
    }
}