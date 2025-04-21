using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using Plugin.BLE;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using SkiaSharp;
using Unity;

namespace v4posme_maui.ViewModels.Abonos;

public class ValidarAbonoFinancieraViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryParameters _repositoryParameters;
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;
    private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization;
    
    public ValidarAbonoFinancieraViewModel()
    {
        Title = "Comprobanto de Abono 5/5";
        _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        _repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
        Item = VariablesGlobales.DtoAplicarAbono!;
        AplicarOtroCommand = new Command(OnAplicarOtroCommand);
        PrintCommand = new Command(OnPrintCommand);
        AnularCommand = new Command(OnAnularCommand);
        ListaCuotasAplicadas = new();
    }

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> ListaCuotasAplicadas
    {
        get;
    }
    
    private int _cuotasPenditens;

    public int CuotasPendientes
    {
        get=>_cuotasPenditens; 
        set=>SetProperty(ref _cuotasPenditens, value);
    }
    
    private TbCompany? _company;

    public TbCompany? Company
    {
        get => _company;
        private set => SetProperty(ref _company, value);
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

    private Api_AppMobileApi_GetDataDownloadDocumentCreditResponse _documentCreditResponse = new();

    public Api_AppMobileApi_GetDataDownloadDocumentCreditResponse DocumentCreditResponse
    {
        get=>_documentCreditResponse; 
        set=>SetProperty(ref _documentCreditResponse, value);
    }
    public ViewTempDtoAbono Item { get; private set; }

    private ImageSource? _logoSource;
    
    private List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> DocumentosConRemanentes = [];
    private List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> DocumentCrediAmortizationHastaHoy = [];

    public ImageSource? LogoSource
    {
        get => _logoSource;
        set => SetProperty(ref _logoSource, value);
    }

    public ICommand AplicarOtroCommand { get; }

    public ICommand PrintCommand { get; }
    public Command AnularCommand { get; }

    private async void OnAnularCommand()
    {
        IsBusy = true;
        var findAbonos = await _repositoryTbTransactionMaster.PosMeFilterAbonosByCustomer(Item.EntityId);
        if (findAbonos.First().TransactionNumber != Item.CodigoAbono)
        {
            ShowToast(Mensajes.AnularAbonoValidacion, ToastDuration.Long, 18);
            IsBusy = false;
            return;
        }

        await HelperCustomerCreditDocumentAmortization.AnularAbono(Item.CodigoAbono);
        OnAplicarOtroCommand();
        IsBusy = false;
    }

    private async void OnPrintCommand(object obj)
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
            ShowToast(Mensajes.MensajeBluetoothState, ToastDuration.Long, 18);
            return;
        }

        IsBusy = true;
        if (!string.IsNullOrWhiteSpace(logo.Value))
        {
            var readImage = Convert.FromBase64String(logo.Value!);
            printer.AlignRight();
            printer.Image(SKBitmap.Decode(readImage));
        }

        var viewTempDtoAbono = VariablesGlobales.DtoAplicarAbono;
        printer.AlignCenter();
        printer.BoldMode(Company!.Name!);
        printer.BoldMode($"RUC: {CompanyRuc!.Value}");
        printer.BoldMode("Comprobante de Abono");
        printer.BoldMode($"{viewTempDtoAbono!.CodigoAbono}");
        printer.NewLine();
        printer.AlignLeft();
        printer.Append($"Le informamos: \n{viewTempDtoAbono!.FirstName} {viewTempDtoAbono.LastName} " +
                       $"con número de cedula {viewTempDtoAbono.Identification} ha realizado un abono a su cuenta.");
        printer.NewLine();
        printer.Append($"No. Documento    : {viewTempDtoAbono.DocumentNumber}");
        printer.Append($"Cuota Pactada    : {DocumentCreditResponse.CuotaPactada:N2}");
        printer.Append($"Cant. Cuotas     : {DocumentCreditResponse.CantidadCuotas:N2}");
        printer.Append($"Cuotas Pend.     : {CuotasPendientes:N2}");
        printer.Append($"Días Mora        : {viewTempDtoAbono.DiasMora}");
        printer.Append($"Monto Mora       : {viewTempDtoAbono.CurrencyName} {viewTempDtoAbono.MontoMora}");
        printer.Append($"Fecha            : {viewTempDtoAbono.Fecha:yyyy-MM-dd}");
        printer.Append($"Recibo           : {viewTempDtoAbono!.CodigoAbono}");
        printer.Append($"Saldo Anterior   : {viewTempDtoAbono.CurrencyName} {viewTempDtoAbono.SaldoInicial:N2}");
        printer.Append($"Monto de abono   : {viewTempDtoAbono.CurrencyName} {viewTempDtoAbono.MontoAplicar:N2}");
        printer.Append($"Saldo Actual     : {viewTempDtoAbono.CurrencyName} {viewTempDtoAbono.SaldoFinal:N2}");
        printer.Append($"Mora pagada      : {viewTempDtoAbono.CurrencyName} {viewTempDtoAbono.MoraPagada:N2}");
        printer.NewLine();
        if (ListaCuotasAplicadas.Count > 0)
        {
            printer.Append("Cuotas Aplicadas");
            foreach (var dc in ListaCuotasAplicadas)
            {
                printer.Append($"NO Cuota {dc.Sequence}: {dc.CurrencyName} {dc.MontoCuota}");
            }
        }
        printer.NewLine();
        printer.Append($"Comentarios: {viewTempDtoAbono.Description}");
        printer.NewLine();
        printer.AlignCenter();
        printer.Append(CompanyTelefono!.Value!);
        printer.Append(Company.Address!);
        printer.FullPaperCut();
        printer.Print();
        if (printer.Device is null)
        {
            ShowToast(Mensajes.MensajeDispositivoNoConectado, ToastDuration.Long, 18);
        }

        IsBusy = false;
    }

    private void OnAplicarOtroCommand()
    {
        var stack = Shell.Current.Navigation.NavigationStack.ToArray();
        for (var i = stack.Length - 1; i > 0; i--)
        {
            Shell.Current.Navigation.RemovePage(stack[i]);
        }
    }

    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            IsBusy = true;
            Navigation = navigation;
            var paramter = await _parameterSystem.PosMeFindLogo();
            if (!string.IsNullOrWhiteSpace(paramter.Value))
            {
                var imageBytes = Convert.FromBase64String(paramter.Value!);
                LogoSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }

            Item = VariablesGlobales.DtoAplicarAbono!;
            await LoadDocumentosCuotas();
            DocumentCreditResponse = await _repositoryDocumentCredit.PosMeFindDocumentNumber(Item.DocumentNumber ?? "");
            DocumentosConRemanentes = await _repositoryDocumentCreditAmortization.PosMeFilterByDocumentNumber(Item.DocumentNumber ?? "");
            DocumentCrediAmortizationHastaHoy = DocumentosConRemanentes.Where(dc => dc.DateApply.Date <= DateTime.Now.Date).ToList();
            CompanyTelefono = await _repositoryParameters.PosMeFindByKey("CORE_PHONE");
            CompanyRuc = await _repositoryParameters.PosMeFindByKey("CORE_COMPANY_IDENTIFIER");
            Company = VariablesGlobales.TbCompany;
            CuotasPendientes = Item.CuotasPendientes;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
            Console.WriteLine(e.Message);
            ShowToast(e.Message, ToastDuration.Long, 18);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadDocumentosCuotas()
    {
        var documentos = Item.Documentos;
        if (!string.IsNullOrWhiteSpace(documentos))
        {
            var pares = documentos.Split(',');
            foreach (var par in pares)
            {
                var partes = par.Split(':');
                var idNumero = int.Parse(partes[0]);
                var montoDecimal = decimal.Parse(partes[1]);
                var dc = await _repositoryDocumentCreditAmortization.PosMeFindByAmortizationId(idNumero);
                dc.CurrencyName = Item.CurrencyName;
                dc.MontoCuota = montoDecimal;
                ListaCuotasAplicadas.Add(dc);
            }
        }
    }
}