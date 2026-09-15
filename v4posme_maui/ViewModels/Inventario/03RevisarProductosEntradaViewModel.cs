namespace v4posme_maui.ViewModels.Inventario;

// Paso 3 del flujo de Entrada: revisar/modificar cantidades, precios y costos.
public class RevisarProductosEntradaViewModel : RevisarProductosInventarioBaseViewModel
{
    public RevisarProductosEntradaViewModel()
    {
        Title = "Compra - Revisar";
    }

    public override string TextoConfirmar => "Confirmar Compra";
    public override bool PermiteEditarPrecio => true;
    public override bool PermiteEditarCosto => true;

    protected override Services.SystemNames.TypeTransaction TipoTransaccion
        => Services.SystemNames.TypeTransaction.TransactionInventarioEntrada;
    protected override bool EsEntrada => true;
    protected override string GenerarCodigo() => Helper.GetCodigoEntrada();

    protected override Task NavegarAVisualizacionAsync()
    {
        return NavigationService.NavigateToAsync<VisualizarEntradaViewModel>();
    }
}
