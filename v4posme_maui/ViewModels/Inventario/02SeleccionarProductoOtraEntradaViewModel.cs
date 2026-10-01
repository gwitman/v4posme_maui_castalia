namespace v4posme_maui.ViewModels.Inventario;

// Paso 2 del flujo de Entrada (Otras entradas): seleccion de productos.
public class SeleccionarProductoOtraEntradaViewModel : SeleccionarProductoInventarioBaseViewModel
{
    public SeleccionarProductoOtraEntradaViewModel()
    {
        Title = "Entrada - Productos";
    }

    public override bool PermiteEditarPrecioCosto => true;

    protected override Task NavegarARevisarAsync()
    {
        return NavigationService.NavigateToAsync<RevisarProductosOtraEntradaViewModel>();
    }
}
