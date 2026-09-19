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
using v4posme_maui.Views.More.CashInflow;

namespace v4posme_maui.ViewModels.More.CashInflow;

public class CashInflowComprobanteViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;

    public CashInflowComprobanteViewModel()
    {
        Title = "Comprobante de Ingreso";
        _parameterSystem               = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();

        Ingreso = VariablesGlobales.DtoCashInflow;

        ImprimirCommand           = new Command(async () => await OnImprimirCommand());
        CompartirCommand          = new Command(OnCompartirRequested);
        NuevoIngresoCommand       = new Command(async () => await OnNuevoIngresoCommand());
        EliminarCommand           = new Command(OnEliminarCommand);
        ConfirmarEliminarCommand  = new Command(async () => await OnConfirmarEliminarCommand());
        CancelarEliminarCommand   = new Command(() => ConfirmarEliminarPopUpShow = false);
    }

    public ICommand ImprimirCommand { get; }
    public ICommand CompartirCommand { get; }
    public ICommand NuevoIngresoCommand { get; }
    public ICommand EliminarCommand { get; }
    public ICommand ConfirmarEliminarCommand { get; }
    public ICommand CancelarEliminarCommand { get; }

    // Evento para que la vista dispare la captura/compartir.
    public event EventHandler? CompartirSolicitado;

    // Evento para que la vista navegue tras eliminar el ingreso (true = eliminado).
    public event EventHandler<bool>? EliminacionCompletada;

    private bool _confirmarEliminarPopUpShow;
    public bool ConfirmarEliminarPopUpShow
    {
        get => _confirmarEliminarPopUpShow;
        set => SetProperty(ref _confirmarEliminarPopUpShow, value);
    }

    public ViewTempDtoCashInflow Ingreso { get; }

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
            printer.BoldMode("COMPROBANTE DE INGRESO");
            printer.NewLine();

            printer.AlignLeft();
            printer.Append($"No: {Ingreso.NumeroIngreso}");
            printer.NewLine();
            printer.Append($"Fecha: {Ingreso.Fecha:yyyy-MM-dd hh:mm:ss tt}");
            printer.NewLine();
            printer.Separator();

            printer.Append($"Moneda: {Ingreso.MonedaNombre}");
            printer.NewLine();
            printer.Append($"Monto: {Ingreso.MontoFormateado}");
            printer.NewLine();
            printer.Append($"Comentario: {Ingreso.Comentario}");
            printer.NewLine();

            if (!string.IsNullOrWhiteSpace(Ingreso.Referencia1))
            {
                printer.Append($"Referencia 1: {Ingreso.Referencia1}");
                printer.NewLine();
            }

            if (!string.IsNullOrWhiteSpace(Ingreso.Referencia2))
            {
                printer.Append($"Referencia 2: {Ingreso.Referencia2}");
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

    private async Task OnNuevoIngresoCommand()
    {
        // Navega a una pantalla de ingreso nueva (campos en limpio) y quita este
        // comprobante de la pila para no acumular pantallas al registrar varios ingresos.
        var comprobantePage = Navigation!.NavigationStack.LastOrDefault();
        await Navigation.PushAsync(new CashInflowPage());
        if (comprobantePage is not null)
            Navigation.RemovePage(comprobantePage);
    }

    private void OnEliminarCommand()
    {
        if (IsBusy) return;

        if (Ingreso.TransactionMasterId <= 0)
        {
            ShowMensajePopUp("No se encontro la transaccion del ingreso a eliminar.");
            return;
        }

        // Se muestra el popup de confirmacion con estilo.
        ConfirmarEliminarPopUpShow = true;
    }

    private async Task OnConfirmarEliminarCommand()
    {
        ConfirmarEliminarPopUpShow = false;

        if (IsBusy) return;

        IsBusy = true;
        try
        {
            var master = await _repositoryTbTransactionMaster.PosMeFindByTransactionId(Ingreso.TransactionMasterId);
            if (master is null)
            {
                ShowMensajePopUp("El ingreso ya no existe.");
                return;
            }

            await _repositoryTbTransactionMaster.PosMeDelete(master);

            ShowToast("Ingreso eliminado correctamente.", ToastDuration.Short, 16);
            EliminacionCompletada?.Invoke(this, true);
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
}
