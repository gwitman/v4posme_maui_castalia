using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using Plugin.BLE;
using v4posme_maui.Models;
using v4posme_maui.Services;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using SkiaSharp;
using Unity;
using v4posme_maui.Views.Printers;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Invoices;

public class PrinterInvoiceViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryParameters _repositoryParameters;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail;
    private readonly HelperCore _helperCore;

    public PrinterInvoiceViewModel()
    {
        Title = "Comprobante Pago Factura";
        _helperCore                         = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _repositoryTbTransactionMaster       = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryTbTransactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        _parameterSystem                     = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters                = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        AplicarOtroCommand   = new Command(OnAplicarOtroCommand);
        PrintCommand         = new Command(OnPrintCommand);
        AnularFacturaCommand = new Command(OnAnularFacturaCommand);
        EditFacturaCommand   = new Command(OnEditarFacturaCommand);
    }

    private async void OnEditarFacturaCommand()
    {
        try
        {
            IsBusy                      = true;
            var objTransactionMaster    = await _repositoryTbTransactionMaster.PosMeFindByTransactionId(VariablesGlobales.DtoInvoice.TransactionMasterId);
            //wg-activar--if (
            //wg-activar--    objTransactionMaster.StatusID == (int)TypeStatusBilling.Register || 
            //wg-activar--    objTransactionMaster.StatusID == (int)TypeStatusBilling.Apply
            //wg-activar--)
            //wg-activar--{
            //wg-activar--    throw new Exception(Mensajes.FacturaEditarNotPermitido);
            //wg-activar--}

            bool permission = await _helperCore.GetPermission(TypeMenuElementID.app_invoice_billing_index, TypePermission.Updated, TypeImpact.All);
            if (!permission && VariablesGlobales.User!.UserId! != Constantes.UserIdAdmin)
            {
                throw new Exception(Mensajes.EditarFacturaNoPermitido);
            }

            await NavigationService.NavigateToAsync<DataInvoicesViewModel>(VariablesGlobales.DtoInvoice.CustomerResponse!.CustomerNumber!);            
            IsBusy = false;

        }
        catch (Exception e)
        {   
            ShowMensajePopUp(e.Message);
            IsBusy = false;
        }
    }
    private async void OnAnularFacturaCommand()
    {
        try
        {
            IsBusy          = true;
            bool permission = await _helperCore.GetPermission(TypeMenuElementID.app_invoice_billing_index, TypePermission.Deleted, TypeImpact.All);
            if (!permission && VariablesGlobales.User!.UserId! != Constantes.UserIdAdmin)
            {
                throw new Exception(Mensajes.AnularFacturaNoPermitido);
            }

            var details = await _repositoryTbTransactionMasterDetail.PosMeItemByTransactionId(TransactionMaster.TransactionMasterId);
            await _repositoryTbTransactionMaster.PosMeDelete(TransactionMaster);
            foreach (var masterDetail in details)
            {
                await _repositoryTbTransactionMasterDetail.PosMeDelete(masterDetail);
            }
            OnAplicarOtroCommand();
            ShowMensajePopUp(Mensajes.AnularFacturaCorrectamente, Colors.Green);
            IsBusy = false;
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 13);            
        }
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

            var dtoInvoice = DtoInvoice;
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

            printer.AlignCenter();
            printer.BoldMode(Company!.Name!);
            printer.BoldMode($"RUC: {CompanyRuc!.Value}");
            printer.BoldMode("FACTURA");
            printer.BoldMode(dtoInvoice.Codigo);
            printer.BoldMode($"FECHA: {dtoInvoice.TransactionOn:yyyy-MM-dd hh:mm tt}");
            printer.NewLine();
            printer.AlignLeft();
            var detalleHeader = $"""
                                 VENDEDOR     :{VariablesGlobales.User!.Nickname}
                                 CODIGO       :{dtoInvoice.CustomerResponse.CustomerNumber}
                                 MONEDA       :{dtoInvoice.Currency!.Simbolo}
                                 TIPO         :{dtoInvoice.TipoDocumento!.Name}
                                 CLIENTE       
                                 {dtoInvoice.NombreCompleto}
                                 {dtoInvoice.Comentarios}
                                 """;
            printer.NewLine();
            printer.Append(detalleHeader);
            printer.NewLine();
            printer.Append("CANT.       PREC         TOTAL");
            foreach (var item in dtoInvoice.Items)
            {
                printer.Append(item.Name);
                printer.Append($"{item.Quantity}        {item.PrecioPublico:N2}         {item.Importe:N2}");
            }

            printer.NewLine();
            printer.Append($"TOTAL:               {dtoInvoice.Balance:N2}");
            printer.Append($"RECIBIDO:            {dtoInvoice.Monto:N2}");
            printer.Append($"CAMBIO:              {dtoInvoice.Cambio:N2}");
            printer.NewLine();
            printer.AlignCenter();
            printer.Append($"{Company.Address}");
            printer.Append($"{CompanyTelefono!.Value}");
            printer.NewLine();
            printer.NewLine();
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
            Debug.WriteLine(e.StackTrace);
            ShowMensajePopUp(e.Message);
        }
    }

    private void OnAplicarOtroCommand()
    {
        var stack = Shell.Current.Navigation.NavigationStack.ToArray();
        for (var i = stack.Length - 1; i > 0; i--)
        {
            Shell.Current.Navigation.RemovePage(stack[i]);
        }
    }
    
    private TbTransactionMaster _transactionMaster;

    public TbTransactionMaster TransactionMaster
    {
        get=>_transactionMaster; 
        set=>SetProperty(ref _transactionMaster,value);
    }
    private ImageSource? _logoSource;

    public ImageSource? LogoSource
    {
        get => _logoSource;
        set => SetProperty(ref _logoSource, value);
    }

    private TbCompany? _company;

    public TbCompany? Company
    {
        get => _company;
        private set => SetProperty(ref _company, value);
    }

    public ViewTempDtoInvoice DtoInvoice => VariablesGlobales.DtoInvoice;
    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> ItemsResponses => VariablesGlobales.DtoInvoice.Items;
    public string Moneda => VariablesGlobales.DtoInvoice.Currency!.Simbolo;
    public string Balance => VariablesGlobales.DtoInvoice.Balance.ToString("N2");
    public string Monto => VariablesGlobales.DtoInvoice.Monto.ToString("N2");
    public string Cambio => VariablesGlobales.DtoInvoice.Cambio.ToString("N2");
    public Command PrintCommand { get; }
    public Command AplicarOtroCommand { get; }
    public string SimboloMoneda => VariablesGlobales.DtoInvoice.Currency!.Simbolo;
    public string NombreCliente => VariablesGlobales.DtoInvoice.NombreCompleto!;
    public string CodigoVendedor => VariablesGlobales.User!.Nickname!;

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

    private bool _enableBackButton;

    public bool EnableBackButton
    {
        get => _enableBackButton;
        set => SetProperty(ref _enableBackButton, value);
    }

    public Command AnularFacturaCommand { get; }
    public Command EditFacturaCommand { get; }

    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            Navigation = navigation;
            var paramter = await _parameterSystem.PosMeFindLogo();
            if (!string.IsNullOrWhiteSpace(paramter.Value))
            {
                var imageBytes = Convert.FromBase64String(paramter.Value!);
                LogoSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }
            CompanyTelefono = await _repositoryParameters.PosMeFindByKey("CORE_PHONE");
            CompanyRuc = await _repositoryParameters.PosMeFindByKey("CORE_COMPANY_IDENTIFIER");
            Company = VariablesGlobales.TbCompany;
            EnableBackButton = VariablesGlobales.EnableBackButton;
            TransactionMaster = DtoInvoice.TransactionMaster;
            IsBusy = false;
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
    }
    
}