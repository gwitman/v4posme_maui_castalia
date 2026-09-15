namespace v4posme_maui.ViewModels.Inventario;

// Paso 3 del flujo de Salida: verificar/modificar cantidades. En la Salida solo se
// permite modificar la cantidad (no precio ni costo).
public class RevisarProductosSalidaViewModel : RevisarProductosInventarioBaseViewModel
{
    public RevisarProductosSalidaViewModel()
    {
        Title = "Salida - Revisar";
    }

    public override string TextoConfirmar => "Confirmar Salida";
    public override bool PermiteEditarPrecio => false;
    public override bool PermiteEditarCosto => false;

    protected override Services.SystemNames.TypeTransaction TipoTransaccion
        => Services.SystemNames.TypeTransaction.TransactionInventarioSalida;
    protected override bool EsEntrada => false;
    protected override string GenerarCodigo() => Helper.GetCodigoSalida();

    protected override Task NavegarAVisualizacionAsync()
    {
        return NavigationService.NavigateToAsync<VisualizarSalidaViewModel>();
    }
}
