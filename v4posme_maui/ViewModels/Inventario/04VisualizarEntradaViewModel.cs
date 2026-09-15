using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.Inventario;

// Paso 4 (final) del flujo de Entrada: visualizacion e insercion en base de datos.
public class VisualizarEntradaViewModel : VisualizarInventarioBaseViewModel
{
    public VisualizarEntradaViewModel()
    {
        Title = "Compra Registrada";
    }

    protected override TypeTransaction TipoTransaccion => TypeTransaction.TransactionInventarioEntrada;
    protected override bool EsEntrada => true;

    protected override string GenerarCodigo() => Helper.GetCodigoEntrada();

    protected override string RutaNuevo => "InventEntrada";
}
