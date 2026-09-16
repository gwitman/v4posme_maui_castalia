using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Inventario;

// Paso 1 del flujo de Entrada de inventario (Compras / TransactionId 21).
// Captura comentario (obligatorio), referencia1 y referencia2. Al dar "Siguiente"
// navega a la seleccion de productos.
public class DatosEntradaViewModel : BaseViewModel
{
    public DatosEntradaViewModel()
    {
        Title                     = "Compra - Datos";
        SiguienteCommand          = new Command(OnSiguiente);
        AbrirMenuPrincipalCommand = new Command(OnAbrirMenuPrincipal);
    }

    public Command SiguienteCommand { get; }
    public Command AbrirMenuPrincipalCommand { get; }

    private void OnAbrirMenuPrincipal()
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }

    private string _comentarios = string.Empty;
    public string Comentarios
    {
        get => _comentarios;
        set => SetProperty(ref _comentarios, value, nameof(Comentarios), () => ErrorComentarios = false);
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

    private bool _errorComentarios;
    public bool ErrorComentarios
    {
        get => _errorComentarios;
        set => SetProperty(ref _errorComentarios, value);
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;

        // Al abrir esta pantalla (primer paso del flujo) se limpia el estado de la
        // transaccion para que los campos y los productos inicien vacios y no queden
        // precargados de un flujo anterior.
        VariablesGlobales.DtoInventario = new ViewTempDtoInventario
        {
            TransactionId = TypeTransaction.TransactionInventarioEntrada
        };

        Comentarios = string.Empty;
        Referencia1 = string.Empty;
        Referencia2 = string.Empty;
        ErrorComentarios = false;
        IsBusy = false;
    }

    private async void OnSiguiente()
    {
        if (string.IsNullOrWhiteSpace(Comentarios))
        {
            ErrorComentarios = true;
            ShowToast("El comentario no puede estar vacío", ToastDuration.Long, 12);
            return;
        }

        VariablesGlobales.DtoInventario.TransactionId = TypeTransaction.TransactionInventarioEntrada;
        VariablesGlobales.DtoInventario.Comentarios   = Comentarios;
        VariablesGlobales.DtoInventario.Referencia1   = Referencia1;
        VariablesGlobales.DtoInventario.Referencia2   = Referencia2;

        await NavigationService.NavigateToAsync<SeleccionarProductoEntradaViewModel>();
    }
}
