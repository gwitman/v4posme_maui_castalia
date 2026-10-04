using System.Collections.ObjectModel;
using System.Windows.Input;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.More.CashInflow;

namespace v4posme_maui.ViewModels.More.CashInflow;

public class CashInflowViewModel : BaseViewModel
{
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly HelperCore _helperCore;

    public ObservableCollection<TypeMoneda> Monedas { get; }

    public CashInflowViewModel()
    {
        Title = "Registrar Ingreso";
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _helperCore                    = VariablesGlobales.UnityContainer.Resolve<HelperCore>();

        Monedas =
        [
            new TypeMoneda { Key = (int)TypeCurrency.Cordoba, Name = "Cordoba (C$)", Simbolo = "C$" },
            new TypeMoneda { Key = (int)TypeCurrency.Dolar,   Name = "Dolar ($)",    Simbolo = "$"  }
        ];
        _monedaSeleccionada = Monedas[0];

        GuardarCommand = new Command(async () => await OnGuardarCommand());
    }

    public ICommand GuardarCommand { get; }

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

    public string MontoFormateado
    {
        get
        {
            var simbolo = MonedaSeleccionada?.Simbolo ?? string.Empty;
            return decimal.TryParse(Monto, out var valor) ? $"{simbolo} {valor:N2}" : $"{simbolo} 0.00";
        }
    }

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        IsBusy = false;

        // Validar permiso antes de mostrar los primeros datos a registrar.
        var permission = await _helperCore.GetPermission(TypeMenuElementID.app_box_inputcash, TypePermission.Updated, TypeImpact.All);
        if (!permission)
        {
            ShowMensajePopUp(Mensajes.MensajeNoTienePermisoDeEdicion);
        }
    }

    private const string Screen = "CashInflow(Ingreso)";

    private async Task OnGuardarCommand()
    {
        HelperLogs.Trace(Screen, "OnGuardarCommand", "inicio");
        if (IsBusy) return;

        // Validar permiso antes de guardar la operacion.
        var permission = await _helperCore.GetPermission(TypeMenuElementID.app_box_inputcash, TypePermission.Updated, TypeImpact.All);
        HelperLogs.Trace(Screen, "OnGuardarCommand", $"permiso de edición = {permission}");
        if (!permission)
        {
            ShowMensajePopUp(Mensajes.MensajeNoTienePermisoDeEdicion);
            return;
        }

        // Validaciones antes de guardar.
        if (!decimal.TryParse(Monto, out var monto) || monto <= decimal.Zero)
        {
            HelperLogs.Trace(Screen, "OnGuardarCommand", $"monto inválido: '{Monto}'", "Warning");
            ShowMensajePopUp("Ingrese un monto valido mayor a cero.");
            return;
        }

        // El comentario es obligatorio para el ingreso.
        if (string.IsNullOrWhiteSpace(Comentario))
        {
            HelperLogs.Trace(Screen, "OnGuardarCommand", "comentario vacío", "Warning");
            ShowMensajePopUp("El comentario es obligatorio para el ingreso.");
            return;
        }

        if (MonedaSeleccionada is null)
        {
            HelperLogs.Trace(Screen, "OnGuardarCommand", "MonedaSeleccionada es null", "Warning");
            ShowMensajePopUp("Seleccione una moneda.");
            return;
        }

        IsBusy = true;
        try
        {
            HelperLogs.TraceValue(Screen, "VariablesGlobales.User", VariablesGlobales.User);
            HelperLogs.Trace(Screen, "OnGuardarCommand", $"monto={monto}, moneda={MonedaSeleccionada.Name}");
            var codigo = await _helperCore.GetCodigoCashInflow();
            HelperLogs.TraceValue(Screen, "codigo", codigo);

            var transactionMaster = new TbTransactionMaster
            {
                TransactionId       = TypeTransaction.TransactionCashInflow,
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

            HelperLogs.Trace(Screen, "OnGuardarCommand", "insertando transacción maestra");
            await _repositoryTbTransactionMaster.PosMeInsert(transactionMaster);
            HelperLogs.Trace(Screen, "OnGuardarCommand", $"insertada (TransactionMasterId={transactionMaster.TransactionMasterId}), actualizando contador");
            await _helperCore.PlusCounter();

            // Se prepara el estado del comprobante para la pantalla de resultado.
            VariablesGlobales.DtoCashInflow = new ViewTempDtoCashInflow
            {
                TransactionMasterId = transactionMaster.TransactionMasterId,
                NumeroIngreso = codigo,
                Fecha         = transactionMaster.TransactionOn,
                MonedaNombre  = MonedaSeleccionada.Name,
                MonedaSimbolo = MonedaSeleccionada.Simbolo,
                Monto         = monto,
                Comentario    = Comentario,
                Referencia1   = Referencia1,
                Referencia2   = Referencia2
            };

            HelperLogs.Trace(Screen, "OnGuardarCommand", "éxito, navegando al comprobante");
            await Navigation!.PushAsync(new CashInflowComprobantePage());
        }
        catch (Exception ex)
        {
            HelperLogs.Trace(Screen, "OnGuardarCommand", $"EXCEPCIÓN: {ex.GetType().Name} - {ex.Message}", "Error");
            HelperLogs.Log(ex);
            ShowMensajePopUp(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public class TypeMoneda
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Simbolo { get; set; } = string.Empty;
    }
}
