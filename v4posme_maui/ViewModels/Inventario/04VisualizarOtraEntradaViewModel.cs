using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.Inventario;

// Paso 4 (final) del flujo de Entrada (Otras entradas): visualizacion e insercion en base de datos.
public class VisualizarOtraEntradaViewModel : VisualizarInventarioBaseViewModel
{
    public VisualizarOtraEntradaViewModel()
    {
        Title = "Entrada Registrada";
    }

    protected override TypeTransaction TipoTransaccion => TypeTransaction.TransactionInventarioEntradas;
    protected override bool EsEntrada => true;

    protected override string RutaNuevo => "InventOtraEntrada";

    protected override string EtiquetaDocumento => "ENTRADA";
}
