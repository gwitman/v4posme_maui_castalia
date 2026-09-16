namespace v4posme_maui.ViewModels.Inventario;

// Paso 2 del flujo de Salida: seleccion de productos (Otras salidas).
public class SeleccionarProductoSalidaViewModel : SeleccionarProductoInventarioBaseViewModel
{
    public SeleccionarProductoSalidaViewModel()
    {
        Title = "Salida - Productos";
    }

    public override bool PermiteEditarPrecioCosto => false;

    protected override Task NavegarARevisarAsync()
    {
        return NavigationService.NavigateToAsync<RevisarProductosSalidaViewModel>();
    }
}
