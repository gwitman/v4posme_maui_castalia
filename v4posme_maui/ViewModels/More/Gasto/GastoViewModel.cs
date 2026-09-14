using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.More.Gasto;

namespace v4posme_maui.ViewModels.More.Gasto;

public class GastoViewModel : BaseViewModel
{
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly HelperCore _helperCore;

    public ObservableCollection<TypeMoneda> Monedas { get; }

    public GastoViewModel()
    {
        Title = "Registrar Gasto";
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

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        IsBusy = false;
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

            // Se prepara el estado del comprobante para la pantalla de resultado.
            VariablesGlobales.DtoGasto = new ViewTempDtoGasto
            {
                TransactionMasterId = transactionMaster.TransactionMasterId,
                NumeroGasto   = codigo,
                Fecha         = transactionMaster.TransactionOn,
                MonedaNombre  = MonedaSeleccionada.Name,
                MonedaSimbolo = MonedaSeleccionada.Simbolo,
                Monto         = monto,
                Comentario    = Comentario,
                Referencia1   = Referencia1,
                Referencia2   = Referencia2
            };

            await Navigation!.PushAsync(new GastoComprobantePage());
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

    public class TypeMoneda
    {
        public int Key { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Simbolo { get; set; } = string.Empty;
    }
}
