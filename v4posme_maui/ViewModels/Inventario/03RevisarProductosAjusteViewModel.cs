namespace v4posme_maui.ViewModels.Inventario;

// Paso 3 del flujo de Ajuste: revisar/modificar cantidades, precios y costos.
public class RevisarProductosAjusteViewModel : RevisarProductosInventarioBaseViewModel
{
    public RevisarProductosAjusteViewModel()
    {
        Title = "Ajuste - Revisar";
    }

    public override string TextoConfirmar => "Confirmar Ajuste";
    public override bool PermiteEditarPrecio => true;
    public override bool PermiteEditarCosto => true;

    protected override Services.SystemNames.TypeTransaction TipoTransaccion
        => Services.SystemNames.TypeTransaction.TransactionInventarioAjuste;
    protected override bool EsEntrada => true;
    protected override string GenerarCodigo() => Helper.GetCodigoAjuste();

    protected override Task NavegarAVisualizacionAsync()
    {
        return NavigationService.NavigateToAsync<VisualizarAjusteViewModel>();
    }
}
