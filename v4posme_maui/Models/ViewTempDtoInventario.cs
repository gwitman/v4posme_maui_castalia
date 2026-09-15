using System.Collections.ObjectModel;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Models;

// Estado temporal del flujo de inventario (Entrada = Compras / Salida = Otras salidas).
// Mantiene los datos del encabezado (comentario, referencias) y los productos que se van
// agregando durante el flujo. Se comparte a traves de VariablesGlobales igual que el
// DtoInvoice de facturacion.
public class ViewTempDtoInventario
{
    public ViewTempDtoInventario()
    {
        Items = new();
    }

    // Tipo de transaccion en curso (Entrada o Salida). Determina el comportamiento y las
    // consultas en la base de datos.
    public TypeTransaction TransactionId { get; set; } = TypeTransaction.TransactionInventarioEntrada;

    public string? Comentarios { get; set; } = string.Empty;

    public string? Referencia1 { get; set; } = string.Empty;

    public string? Referencia2 { get; set; } = string.Empty;

    public int CantidadTotalSeleccionada { get; set; }

    public decimal Balance { get; set; }

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Items { get; }

    public string Codigo { get; set; } = string.Empty;

    public DateTime TransactionOn { get; set; } = DateTime.Now;

    public int TransactionMasterId { get; set; }

    public TbTransactionMaster? TransactionMaster { get; set; }

    // Indica que la visualizacion fue abierta desde la pantalla de Impresiones (solo
    // lectura de un registro existente). En ese caso la pantalla debe permitir regresar
    // al listado (tabs) con el boton atras, en lugar de comportarse como paso final del
    // flujo de creacion.
    public bool AbiertoDesdeImpresiones { get; set; }

    public void ClearItems()
    {
        Items.Clear();
    }
}
