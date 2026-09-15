using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.Inventario;

// Paso 4 (final) del flujo de Salida: visualizacion e insercion en base de datos.
public class VisualizarSalidaViewModel : VisualizarInventarioBaseViewModel
{
    public VisualizarSalidaViewModel()
    {
        Title = "Salida Registrada";
    }

    protected override TypeTransaction TipoTransaccion => TypeTransaction.TransactionInventarioSalida;
    protected override bool EsEntrada => false;

    protected override string GenerarCodigo() => Helper.GetCodigoSalida();

    protected override string RutaNuevo => "InventSalida";
}
