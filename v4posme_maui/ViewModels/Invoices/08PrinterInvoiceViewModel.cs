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
using v4posme_maui.Services.HelpersPrinters.Helper;

namespace v4posme_maui.ViewModels.Invoices;

public class PrinterInvoiceViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryParameters _repositoryParameters;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail;
    private readonly HelperCore _helperCore;
    private readonly IRepositoryItems _repositoryItems;
    private readonly IRepositoryTbCustomer _repositoryTbCustomer;
    private readonly RestApiAppMobileApi _restApiDownload = new RestApiAppMobileApi();

    public PrinterInvoiceViewModel()
    {
        Title = "Comprobante Pago Factura";
        _helperCore                          = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _repositoryTbTransactionMaster       = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryTbTransactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        _parameterSystem                     = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters                = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _repositoryItems                     = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _repositoryTbCustomer                = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        AplicarOtroCommand   = new Command(OnAplicarOtroCommand);
        PrintCommand         = new Command(OnPrintCommand);
        AnularFacturaCommand = new Command(OnAnularFacturaCommand);
        EditFacturaCommand   = new Command(OnEditarFacturaCommand);
        SubirCommand         = new Command(OnSubirCommand);
    }

    private async void OnEditarFacturaCommand()
    {
        try
        {
            IsBusy                      = true;
            var objTransactionMaster    = await _repositoryTbTransactionMaster.PosMeFindByTransactionId(VariablesGlobales.DtoInvoice.TransactionMasterId);
            if (
                objTransactionMaster.StatusID == (int)TypeStatusBilling.Anulada || 
                objTransactionMaster.StatusID == (int)TypeStatusBilling.Apply
            )
            {
                throw new Exception(Mensajes.FacturaEditarNotPermitido);
            }

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
            string printerQR        = await _helperCore.GetValueParameter("MOBILE_PRINTER_QR_IN_INVOICE", "false");
            string printerQRUrl     = await _helperCore.GetValueParameter("MOBILE_PRINTER_QR_IN_INVOICE_URL", "www.google.com");
            string printerFormat    = await _helperCore.GetValueParameter("MOBILE_PRINTER_INVOICE_FISICAL_FORMAT", "default");
            var parametroPrinter    = await _parameterSystem.PosMeFindPrinter();
            var logo                = await _parameterSystem.PosMeFindLogo();
            if (string.IsNullOrWhiteSpace(parametroPrinter.Value))
            {
                return;
            }

            var dtoInvoice = DtoInvoice;
            var printer    = new Printer(parametroPrinter.Value);
            if (!CrossBluetoothLE.Current.IsOn)
            {
                ShowMensajePopUp(Mensajes.MensajeBluetoothState);
                return;
            }

            IsBusy = true;

            if (printerFormat.Trim().Equals("default", StringComparison.OrdinalIgnoreCase))
            {
                string printerReferencia = await _helperCore.GetValueParameter("PRINTER_REFERENCE_INVOICE_MOBILE", "false");
                await PrinterInvoiceFormatHelper.PrintDefaultFormat(
                    printer,
                    dtoInvoice,
                    logo,
                    Company!.Name!,
                    CompanyRuc!.Value!,
                    Company.Address!,
                    CompanyTelefono!.Value!,
                    printerQR,
                    printerQRUrl,
                    printerReferencia,
                    VariablesGlobales.User!.Nickname!,
                    _helperCore
                );
            }
            if (printerFormat.Trim().Equals("loto", StringComparison.OrdinalIgnoreCase))
            {
                string printerReferencia = await _helperCore.GetValueParameter("PRINTER_REFERENCE_INVOICE_MOBILE", "false");
                await PrinterInvoiceFormatHelper.PrintLotoFormat(
                    printer,
                    dtoInvoice,
                    logo,
                    Company!.Name!,
                    CompanyRuc!.Value!,
                    Company.Address!,
                    CompanyTelefono!.Value!,
                    printerQR,
                    printerQRUrl,
                    printerReferencia,
                    VariablesGlobales.User!.Nickname!,
                    _helperCore
                );
            }

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

    private async void OnSubirCommand()
    {
        var uploadAfterInvoiceValue = await _helperCore.GetValueParameter("MOBILE_UPLOAD_AFTER_INVOICE", "false");
        var uploadAfterInvoice      = bool.TryParse(uploadAfterInvoiceValue, out var parsedValue) && parsedValue;
        if (uploadAfterInvoice )
        {
            IsBusy = true;

            // Subir datos
            var response    = await _restApiDownload.SendDataAsync();
            var apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Api_AppMobileApi_SetDataUploadResponse>(response);
            if (apiResponse is not null)
            {
                if (apiResponse.Error)
                {
                    ShowMensajePopUp($"{Mensajes.MensajeUploadError} {apiResponse.Message}", Colors.Red);
                }
                else
                {
                    Mensaje              = Mensajes.MensajeUploadSuccess;
                    PopupBackgroundColor = Colors.Green;
                    PopUpShow            = true;
                    await _repositoryItems.PosMeDeleteAll();
                    await _repositoryTbCustomer.PosMeDeleteAll();
                    await _repositoryTbTransactionMaster.PosMeDeleteAll();
                    await _repositoryTbTransactionMasterDetail.PosMeDeleteAll();
                    await _helperCore.ZeroCounter();
                }
            }
            else
            {
                ShowMensajePopUp(Mensajes.MensajeUploadError, Colors.Red);
            }

            // Descargar datos
            var counter = await _helperCore.GetCounter();
            if (counter != 0)
                await _restApiDownload.GetDataDownload(true);
            else
                await _restApiDownload.GetDataDownload(false);

            IsBusy = false;
        }

        // Navegar a página 1 (selección de cliente)
        OnAplicarOtroCommand();
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
    public string Moneda => VariablesGlobales.DtoInvoice.Currency!.Simbolo + " ";
    public string Balance => VariablesGlobales.DtoInvoice.Balance.ToString("N2");
    public string Descuento => VariablesGlobales.DtoInvoice.TransactionMaster.Discount.ToString("N2");
    public string Amount => VariablesGlobales.DtoInvoice.TransactionMaster.Amount.ToString("N2");
    public string SubAmount => VariablesGlobales.DtoInvoice.TransactionMaster.SubAmount.ToString("N2");

    public string Monto => VariablesGlobales.DtoInvoice.Monto.ToString("N2");
    public string Cambio => VariablesGlobales.DtoInvoice.Cambio.ToString("N2");
    public Command PrintCommand { get; }
    public Command AplicarOtroCommand { get; }
    public Command SubirCommand { get; }
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

    private bool _showQr;

    public bool ShowQr
    {
        get => _showQr;
        set => SetProperty(ref _showQr, value);
    }

    private bool _showReferencia;

    public bool ShowReferencia
    {
        get => _showReferencia;
        set => SetProperty(ref _showReferencia, value);
    }

    private ImageSource? _qrImageSource;

    public ImageSource? QrImageSource
    {
        get => _qrImageSource;
        set => SetProperty(ref _qrImageSource, value);
    }

    public Command AnularFacturaCommand { get; }
    public Command EditFacturaCommand { get; }

    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            Navigation          = navigation;
            var paramter        = await _parameterSystem.PosMeFindLogo();
            byte[]? logoBytes   = null;
            if (!string.IsNullOrWhiteSpace(paramter.Value))
            {
                logoBytes       = Convert.FromBase64String(paramter.Value!);
                LogoSource      = ImageSource.FromStream(() => new MemoryStream(logoBytes));
                
                Debug.WriteLine($"[LOGO] LogoSource assigned, bytes: {logoBytes.Length}");
                Debug.WriteLine($"[QR] QrImageSource assigned, bytes: {logoBytes.Length}");
            }
            CompanyTelefono     = await _repositoryParameters.PosMeFindByKey("CORE_PHONE");
            CompanyRuc          = await _repositoryParameters.PosMeFindByKey("CORE_COMPANY_IDENTIFIER");
            Company             = VariablesGlobales.TbCompany;
            EnableBackButton    = VariablesGlobales.EnableBackButton;
            TransactionMaster   = DtoInvoice.TransactionMaster;

            string printerQR    = await _helperCore.GetValueParameter("MOBILE_PRINTER_QR_IN_INVOICE", "false");
            string printerQRUrl = await _helperCore.GetValueParameter("MOBILE_PRINTER_QR_IN_INVOICE_URL", "www.google.com");
            Debug.WriteLine($"[QR] MOBILE_PRINTER_QR_IN_INVOICE = '{printerQR}'");
            ShowQr              = printerQR.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);
            Debug.WriteLine($"[QR] ShowQr = {ShowQr}");
            if(ShowQr == true)
            {
                string qrContent = printerQRUrl + "/inm/" + DtoInvoice.Codigo + "/unm/" + VariablesGlobales.User!.Nickname;
                QrImageSource    = GenerateQrImageSource(qrContent);
            }

            string printerRef   = await _helperCore.GetValueParameter("PRINTER_REFERENCE_INVOICE_MOBILE", "false");
            ShowReferencia      = printerRef.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);            
            IsBusy              = false;
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
    }

    private static ImageSource GenerateTestImageSource()
    {
        // Crear una imagen de prueba simple: cuadrado rojo de 200x200
        var bitmap = new SKBitmap(200, 200);
        using (var canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.White);
            var paint = new SKPaint
            {
                Color = SKColors.Red,
                Style = SKPaintStyle.Fill
            };
            canvas.DrawRect(50, 50, 100, 100, paint);
        }

        byte[] bytes;
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            bytes = data.ToArray();
        }
        bitmap.Dispose();

        Debug.WriteLine($"[TEST] PNG bytes generated: {bytes.Length}");
        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }

    private static ImageSource GenerateQrImageSource(string content, int size = 300)
    {
        var writer = new ZXing.BarcodeWriterPixelData
        {
            Format  = ZXing.BarcodeFormat.QR_CODE,
            Options = new ZXing.Common.EncodingOptions { Width = size, Height = size, Margin = 1 }
        };
        var pixelData = writer.Write(content);
        var bitmap    = new SKBitmap(pixelData.Width, pixelData.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
        System.Runtime.InteropServices.Marshal.Copy(pixelData.Pixels, 0, bitmap.GetPixels(), pixelData.Pixels.Length);

        byte[] bytes;
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data  = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            bytes = data.ToArray();
        }
        bitmap.Dispose();

        Debug.WriteLine($"[QR] PNG bytes generated: {bytes.Length}");
        return ImageSource.FromStream(() => new MemoryStream(bytes));
    }
    
}