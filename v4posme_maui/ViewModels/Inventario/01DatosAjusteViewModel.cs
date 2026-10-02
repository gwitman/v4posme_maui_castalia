using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Inventario;

// Paso 1 del flujo de Ajuste de inventario (TransactionId 33).
// Captura comentario (obligatorio), referencia1 y referencia2. Al dar "Siguiente"
// navega a la seleccion de productos.
public class DatosAjusteViewModel : BaseViewModel
{
    private readonly HelperCore _helperContador;

    public DatosAjusteViewModel()
    {
        Title                     = "Ajuste - Datos";
        _helperContador           = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
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

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        IsBusy     = false;

        // Validar permiso antes de mostrar los primeros datos a registrar.
        var permission = await _helperContador.GetPermission(TypeMenuElementID.app_inventory_ajuste, TypePermission.Updated, TypeImpact.All);
        if (!permission)
        {
            ShowToast(Mensajes.MensajeNoTienePermisoDeEdicion, ToastDuration.Long, 14);
            return;
        }

        // Al abrir esta pantalla (primer paso del flujo) se limpia el estado de la
        // transaccion para que los campos y los productos inicien vacios y no queden
        // precargados de un flujo anterior.
        VariablesGlobales.DtoInventario = new ViewTempDtoInventario
        {
            TransactionId = TypeTransaction.TransactionInventarioAjuste
        };

        Comentarios = string.Empty;
        Referencia1 = string.Empty;
        Referencia2 = string.Empty;
        ErrorComentarios = false;
        IsBusy = false;
    }

    private async void OnSiguiente()
    {
        // Validar permiso antes de guardar/continuar la operacion.
        var permission = await _helperContador.GetPermission(TypeMenuElementID.app_inventory_ajuste, TypePermission.Updated, TypeImpact.All);
        if (!permission)
        {
            ShowToast(Mensajes.MensajeNoTienePermisoDeEdicion, ToastDuration.Long, 14);
            return;
        }

        if (string.IsNullOrWhiteSpace(Comentarios))
        {
            ErrorComentarios = true;
            ShowToast("El comentario no puede estar vacío", ToastDuration.Long, 12);
            return;
        }

        VariablesGlobales.DtoInventario.TransactionId = TypeTransaction.TransactionInventarioAjuste;
        VariablesGlobales.DtoInventario.Comentarios   = Comentarios;
        VariablesGlobales.DtoInventario.Referencia1   = Referencia1;
        VariablesGlobales.DtoInventario.Referencia2   = Referencia2;

        await NavigationService.NavigateToAsync<SeleccionarProductoAjusteViewModel>();
    }
}
