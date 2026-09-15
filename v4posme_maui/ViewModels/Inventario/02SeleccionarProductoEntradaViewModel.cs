namespace v4posme_maui.ViewModels.Inventario;

// Paso 2 del flujo de Entrada: seleccion de productos (Compras).
public class SeleccionarProductoEntradaViewModel : SeleccionarProductoInventarioBaseViewModel
{
    public SeleccionarProductoEntradaViewModel()
    {
        Title = "Compra - Productos";
    }

    protected override Task NavegarARevisarAsync()
    {
        return NavigationService.NavigateToAsync<RevisarProductosEntradaViewModel>();
    }
}
