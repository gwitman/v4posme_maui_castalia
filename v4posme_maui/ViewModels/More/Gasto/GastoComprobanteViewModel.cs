using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using Plugin.BLE;
using SkiaSharp;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.More.Gasto;

namespace v4posme_maui.ViewModels.More.Gasto;

public class GastoComprobanteViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;

    public GastoComprobanteViewModel()
    {
        Title = "Comprobante de Gasto";
        _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();

        Gasto = VariablesGlobales.DtoGasto;

        ImprimirCommand   = new Command(async () => await OnImprimirCommand());
        CompartirCommand  = new Command(OnCompartirRequested);
        NuevoGastoCommand = new Command(async () => await OnNuevoGastoCommand());
    }

    public ICommand ImprimirCommand { get; }
    public ICommand CompartirCommand { get; }
    public ICommand NuevoGastoCommand { get; }

    // Evento para que la vista dispare la captura/compartir.
    public event EventHandler? CompartirSolicitado;

    public ViewTempDtoGasto Gasto { get; }

    private TbCompany? _empresa;
    public TbCompany? Empresa
    {
        get => _empresa;
        private set => SetProperty(ref _empresa, value);
    }

    private ImageSource? _logoSource;
    public ImageSource? LogoSource
    {
        get => _logoSource;
        private set => SetProperty(ref _logoSource, value);
    }

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        IsBusy = true;
        try
        {
            Empresa = VariablesGlobales.TbCompany;

            var logo = await _parameterSystem.PosMeFindLogo();
            if (!string.IsNullOrWhiteSpace(logo.Value))
            {
                var bytes = Convert.FromBase64String(logo.Value!);
                LogoSource = ImageSource.FromStream(() => new MemoryStream(bytes));
            }
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task OnImprimirCommand()
    {
        var parametroPrinter = await _parameterSystem.PosMeFindPrinter();
        if (string.IsNullOrWhiteSpace(parametroPrinter.Value))
        {
            ShowMensajePopUp("No hay impresora configurada.");
            return;
        }

        if (!CrossBluetoothLE.Current.IsOn)
        {
            ShowToast(Mensajes.MensajeBluetoothState, ToastDuration.Long, 18);
            return;
        }

        IsBusy = true;
        try
        {
            var printer = new Printer(parametroPrinter.Value);
            var logo    = await _parameterSystem.PosMeFindLogo();

            if (!string.IsNullOrWhiteSpace(logo.Value))
            {
                var readImage = Convert.FromBase64String(logo.Value!);
                printer.AlignCenter();
                printer.Image(SKBitmap.Decode(readImage));
            }

            printer.AlignCenter();
            printer.BoldMode(Empresa?.Name ?? string.Empty);
            printer.BoldMode("COMPROBANTE DE GASTO");
            printer.NewLine();

            printer.AlignLeft();
            printer.Append($"No: {Gasto.NumeroGasto}");
            printer.NewLine();
            printer.Append($"Fecha: {Gasto.Fecha:yyyy-MM-dd hh:mm:ss tt}");
            printer.NewLine();
            printer.Separator();

            printer.Append($"Moneda: {Gasto.MonedaNombre}");
            printer.NewLine();
            printer.Append($"Monto: {Gasto.MontoFormateado}");
            printer.NewLine();
            printer.Append($"Comentario: {Gasto.Comentario}");
            printer.NewLine();

            if (!string.IsNullOrWhiteSpace(Gasto.Referencia1))
            {
                printer.Append($"Referencia 1: {Gasto.Referencia1}");
                printer.NewLine();
            }

            if (!string.IsNullOrWhiteSpace(Gasto.Referencia2))
            {
                printer.Append($"Referencia 2: {Gasto.Referencia2}");
                printer.NewLine();
            }

            printer.Separator();
            printer.NewLines(2);
            printer.FullPaperCut();
            printer.Print();

            if (printer.Device is null)
            {
                ShowToast(Mensajes.MensajeDispositivoNoConectado, ToastDuration.Long, 18);
            }
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
            ShowMensajePopUp(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnCompartirRequested()
    {
        CompartirSolicitado?.Invoke(this, EventArgs.Empty);
    }

    private async Task OnNuevoGastoCommand()
    {
        // Reemplaza la pila para volver a registrar un gasto limpio.
        await Navigation!.PushAsync(new GastoPage());
    }
}
