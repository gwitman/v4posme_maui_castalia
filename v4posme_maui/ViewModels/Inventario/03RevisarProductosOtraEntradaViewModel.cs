namespace v4posme_maui.ViewModels.Inventario;

// Paso 3 del flujo de Entrada (Otras entradas): revisar/modificar cantidades, precios y costos.
public class RevisarProductosOtraEntradaViewModel : RevisarProductosInventarioBaseViewModel
{
    public RevisarProductosOtraEntradaViewModel()
    {
        Title = "Entrada - Revisar";
    }

    public override string TextoConfirmar => "Confirmar Entrada";
    public override bool PermiteEditarPrecio => true;
    public override bool PermiteEditarCosto => true;

    protected override Services.SystemNames.TypeTransaction TipoTransaccion
        => Services.SystemNames.TypeTransaction.TransactionInventarioEntradas;
    protected override bool EsEntrada => true;
    protected override string GenerarCodigo() => Helper.GetCodigoOtraEntrada();

    protected override Task NavegarAVisualizacionAsync()
    {
        return NavigationService.NavigateToAsync<VisualizarOtraEntradaViewModel>();
    }
}
