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

public class ValidarAbonoHideSaldoViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryParameters _repositoryParameters;

    public ValidarAbonoHideSaldoViewModel()
    {
        Title = "Comprobanto de Abono 5/5";
        _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        Item = VariablesGlobales.DtoAplicarAbono!;
        AplicarOtroCommand = new Command(OnAplicarOtroCommand);
        PrintCommand = new Command(OnPrintCommand);
        AnularCommand = new Command(OnAnularCommand);
    }

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
       
        printer.AlignCenter();
        printer.BoldMode(Company!.Name!);
        printer.BoldMode($"RUC: {CompanyRuc!.Value}");
        printer.BoldMode($"ABONO: {VariablesGlobales.DtoAplicarAbono!.CodigoAbono}");
        printer.NewLine();
        printer.AlignLeft();
        printer.Append($"Le informamos: \n{VariablesGlobales.DtoAplicarAbono.FirstName} {VariablesGlobales.DtoAplicarAbono.LastName} " +
                       $"con número de cedula {VariablesGlobales.DtoAplicarAbono.Identification} ha realizado un abono a su cuenta.");
        printer.NewLine();
        printer.Append($"Fecha            : {VariablesGlobales.DtoAplicarAbono.Fecha:yyyy-MM-dd}");
        //printer.Append($"Saldo inicial    : {VariablesGlobales.DtoAplicarAbono.CurrencyName} {VariablesGlobales.DtoAplicarAbono.SaldoInicial:N2}");
        printer.Append($"Monto de abono   : {VariablesGlobales.DtoAplicarAbono.CurrencyName} {VariablesGlobales.DtoAplicarAbono.MontoAplicar:N2}");
       //printer.Append($"Saldo final      : {VariablesGlobales.DtoAplicarAbono.CurrencyName} {VariablesGlobales.DtoAplicarAbono.SaldoFinal:N2}");
        printer.NewLine();
        printer.Append($"Comentarios: {VariablesGlobales.DtoAplicarAbono.Description}");
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

    public ViewTempDtoAbono Item { get; private set; }

    private ImageSource? _logoSource;

    public ImageSource? LogoSource
    {
        get => _logoSource;
        set => SetProperty(ref _logoSource, value);
    }

    public ICommand AplicarOtroCommand { get; }

    public ICommand PrintCommand { get; }
    public Command AnularCommand { get; }

    public override async Task InitializeAsync(object parameter)
    {
        await OnAppearing(Navigation!);
    }


    public async Task OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        await Task.Run(async () =>
        {
            var paramter = await _parameterSystem.PosMeFindLogo();
            var imageBytes = Convert.FromBase64String(paramter.Value!);
            LogoSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            Item = VariablesGlobales.DtoAplicarAbono!;
            CompanyTelefono = await _repositoryParameters.PosMeFindByKey("CORE_PHONE");
            CompanyRuc = await _repositoryParameters.PosMeFindByKey("CORE_COMPANY_IDENTIFIER");
            Company = VariablesGlobales.TbCompany;
        });
        IsBusy = false;
    }
}