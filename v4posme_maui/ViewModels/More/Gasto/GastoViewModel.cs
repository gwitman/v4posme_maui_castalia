using System.Collections.ObjectModel;
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

namespace v4posme_maui.ViewModels.More.Gasto;

public class GastoViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly HelperCore _helperCore;

    public ObservableCollection<TypeMoneda> Monedas { get; }

    public GastoViewModel()
    {
        Title = "Registrar Gasto";
        _parameterSystem               = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _helperCore                    = VariablesGlobales.UnityContainer.Resolve<HelperCore>();

        Monedas =
        [
            new TypeMoneda { Key = (int)TypeCurrency.Cordoba, Name = "Cordoba (C$)", Simbolo = "C$" },
            new TypeMoneda { Key = (int)TypeCurrency.Dolar,   Name = "Dolar ($)",    Simbolo = "$"  }
        ];
        _monedaSeleccionada = Monedas[0];

        GuardarCommand  = new Command(async () => await OnGuardarCommand());
        ImprimirCommand = new Command(async () => await OnImprimirCommand());
        CompartirCommand = new Command(OnCompartirRequested);
    }

    public ICommand GuardarCommand { get; }
    public ICommand ImprimirCommand { get; }
    public ICommand CompartirCommand { get; }

    // Evento para que la vista dispare la captura/compartir (patron usado en las demas pantallas).
    public event EventHandler? CompartirSolicitado;

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

    private TypeMoneda _monedaSeleccionada;
    public TypeMoneda MonedaSeleccionada
    {
        get => _monedaSeleccionada;
        set
        {
            if (SetProperty(ref _monedaSeleccionada, value))
                OnPropertyChanged(nameof(MontoFormateado));
        }
    }

    private string _monto = string.Empty;
    public string Monto
    {
        get => _monto;
        set
        {
            if (SetProperty(ref _monto, value))
                OnPropertyChanged(nameof(MontoFormateado));
        }
    }

    private string _comentario = string.Empty;
    public string Comentario
    {
        get => _comentario;
        set => SetProperty(ref _comentario, value);
    }

    private string _referencia1 = string.Empty;
    public string Referencia1
    {
        get => _referencia1;
        set => SetProperty(ref _referencia1, value);
    }

    private string _referencia2 = string.Empty;
    public string Referencia2
    {
        get => _referencia2;
        set => SetProperty(ref _referencia2, value);
    }

    private bool _isGuardado;
    public bool IsGuardado
    {
        get => _isGuardado;
        private set => SetProperty(ref _isGuardado, value);
    }

    private string _numeroGasto = string.Empty;
    public string NumeroGasto
    {
        get => _numeroGasto;
        private set => SetProperty(ref _numeroGasto, value);
    }

    private DateTime _fechaGasto = DateTime.Now;
    public DateTime FechaGasto
    {
        get => _fechaGasto;
        private set => SetProperty(ref _fechaGasto, value);
    }

    public string MontoFormateado
    {
        get
        {
            var simbolo = MonedaSeleccionada?.Simbolo ?? string.Empty;
            return decimal.TryParse(Monto, out var valor) ? $"{simbolo} {valor:N2}" : $"{simbolo} 0.00";
        }
    }

    public async Task OnAppearing(INavigation navigation)
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

    private async Task OnGuardarCommand()
    {
        if (IsBusy) return;

        // Validaciones antes de guardar.
        if (!decimal.TryParse(Monto, out var monto) || monto <= decimal.Zero)
        {
            ShowMensajePopUp("Ingrese un monto valido mayor a cero.");
            return;
        }

        if (string.IsNullOrWhiteSpace(Comentario))
        {
            ShowMensajePopUp("Ingrese un comentario para el gasto.");
            return;
        }

        if (MonedaSeleccionada is null)
        {
            ShowMensajePopUp("Seleccione una moneda.");
            return;
        }

        IsBusy = true;
        try
        {
            var codigo = await _helperCore.GetCodigoGasto();

            var transactionMaster = new TbTransactionMaster
            {
                TransactionId       = TypeTransaction.TransactionExpense,
                TypePaymentId       = TypePayment.Efectivo,
                TransactionNumber   = codigo,
                TransactionOn       = DateTime.Now,
                EntitySecondaryId   = VariablesGlobales.User?.UserId.ToString(),
                CurrencyId          = (TypeCurrency)MonedaSeleccionada.Key,
                ExchangeRate        = VariablesGlobales.TipoCambio,
                SubAmount           = monto,
                Amount              = monto,
                Discount            = decimal.Zero,
                Taxi1               = decimal.Zero,
                Comment             = Comentario,
                Reference1          = Referencia1,
                Reference2          = Referencia2,
                StatusID            = 1,
                RegisterLocal       = 1
            };

            await _repositoryTbTransactionMaster.PosMeInsert(transactionMaster);
            await _helperCore.PlusCounter();

            NumeroGasto = codigo;
            FechaGasto  = transactionMaster.TransactionOn;
            IsGuardado  = true;

            ShowToast("Gasto registrado correctamente.", ToastDuration.Short, 16);
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

    private async Task OnImprimirCommand()
    {
        if (!IsGuardado)
        {
            ShowMensajePopUp("Guarde el gasto antes de imprimir.");
            return;
        }

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
            printer.BoldMode(Company?.Name ?? string.Empty);
            printer.BoldMode("COMPROBANTE DE GASTO");
            printer.NewLine();

            printer.AlignLeft();
            printer.Append($"No: {NumeroGasto}");
            printer.NewLine();
            printer.Append($"Fecha: {FechaGasto:yyyy-MM-dd hh:mm:ss tt}");
            printer.NewLine();
            printer.Separator();

            printer.Append($"Moneda: {MonedaSeleccionada.Name}");
            printer.NewLine();
            printer.Append($"Monto: {MontoFormateado}");
            printer.NewLine();
            printer.Append($"Comentario: {Comentario}");
            printer.NewLine();

            if (!string.IsNullOrWhiteSpace(Referencia1))
            {
                printer.Append($"Referencia 1: {Referencia1}");
                printer.NewLine();
            }

            if (!string.IsNullOrWhiteSpace(Referencia2))
            {
                printer.Append($"Referencia 2: {Referencia2}");
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
        if (!IsGuardado)
        {
            ShowMensajePopUp("Guarde el gasto antes de compartir.");
            return;
        }

        CompartirSolicitado?.Invoke(this, EventArgs.Empty);
    }

    public class TypeMoneda
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Simbolo { get; set; } = string.Empty;
    }
}
