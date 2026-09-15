using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.Inventario;

// Base compartida para el paso de revision de cantidades, precios y costos en los
// flujos de inventario. Los editores se enlazan directamente a cada producto; este
// view model recalcula el total y confirma la transaccion.
public abstract class RevisarProductosInventarioBaseViewModel : BaseViewModel
{
    protected RevisarProductosInventarioBaseViewModel()
    {
        ProductosSeleccionados = VariablesGlobales.DtoInventario.Items;
        RecalcularCommand      = new Command(RecalcularTotales);
        ConfirmarCommand       = new Command(OnConfirmar);
        AtrasCommand           = new Command(OnAtras);
    }

    // Texto del boton confirmar (Confirmar Compra / Confirmar Salida).
    public abstract string TextoConfirmar { get; }

    // Indica si en este flujo se puede editar el precio publico. En la Entrada se
    // permiten cantidad, costo y precio; en la Salida solo cantidad.
    public abstract bool PermiteEditarPrecio { get; }
    public abstract bool PermiteEditarCosto { get; }

    // Navegacion a la pantalla de visualizacion (implementada por cada flujo).
    protected abstract Task NavegarAVisualizacionAsync();

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> ProductosSeleccionados { get; }

    public Command RecalcularCommand { get; }
    public Command ConfirmarCommand { get; }
    public Command AtrasCommand { get; }

    public string MonedaSimbolo => "C$";

    public decimal Total => ProductosSeleccionados.Sum(r => r.PrecioPublico * r.Quantity);

    public int CantidadTotalItems => (int)ProductosSeleccionados.Sum(r => r.Quantity);

    protected void RecalcularTotales()
    {
        // Eliminar de la lista los productos cuya cantidad quede en 0 (o menos) y
        // actualizar los contadores y totales del DTO.
        var aEliminar = ProductosSeleccionados.Where(r => r.Quantity <= 0).ToList();
        foreach (var item in aEliminar)
        {
            ProductosSeleccionados.Remove(item);
        }

        foreach (var item in ProductosSeleccionados)
        {
            item.Importe = item.PrecioPublico * item.Quantity;
        }

        VariablesGlobales.DtoInventario.Balance                   = ProductosSeleccionados.Sum(r => r.Importe);
        VariablesGlobales.DtoInventario.CantidadTotalSeleccionada = (int)ProductosSeleccionados.Sum(r => r.Quantity);
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CantidadTotalItems));
    }

    private async void OnAtras()
    {
        await NavigationService.GoBackAsync();
    }

    private async void OnConfirmar()
    {
        RecalcularTotales();

        if (ProductosSeleccionados.Any(p => p.Quantity <= 0))
        {
            ShowToast("Las cantidades deben ser mayores a cero", ToastDuration.Long, 12);
            return;
        }

        try
        {
            IsBusy = true;
            await NavegarAVisualizacionAsync();
        }
        catch (Exception ex)
        {
            ShowToast($"Error al navegar: {ex.Message}", ToastDuration.Long, 12);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        RecalcularTotales();
        IsBusy = false;
    }
}
