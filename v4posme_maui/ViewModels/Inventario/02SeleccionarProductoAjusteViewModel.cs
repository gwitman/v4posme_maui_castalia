namespace v4posme_maui.ViewModels.Inventario;

// Paso 2 del flujo de Ajuste: seleccion de productos.
public class SeleccionarProductoAjusteViewModel : SeleccionarProductoInventarioBaseViewModel
{
    public SeleccionarProductoAjusteViewModel()
    {
        Title = "Ajuste - Productos";
    }

    public override bool PermiteEditarPrecioCosto => true;

    protected override Task NavegarARevisarAsync()
    {
        return NavigationService.NavigateToAsync<RevisarProductosAjusteViewModel>();
    }
}
