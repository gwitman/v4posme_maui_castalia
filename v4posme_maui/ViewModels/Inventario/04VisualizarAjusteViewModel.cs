using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.Inventario;

// Paso 4 (final) del flujo de Ajuste: visualizacion e insercion en base de datos.
public class VisualizarAjusteViewModel : VisualizarInventarioBaseViewModel
{
    public VisualizarAjusteViewModel()
    {
        Title = "Ajuste Registrado";
    }

    protected override TypeTransaction TipoTransaccion => TypeTransaction.TransactionInventarioAjuste;
    protected override bool EsEntrada => true;

    protected override string RutaNuevo => "InventAjuste";

    protected override string EtiquetaDocumento => "AJUSTE";
}
