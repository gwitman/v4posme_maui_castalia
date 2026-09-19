namespace v4posme_maui.Models;

// Elemento del listado de ingresos mostrado en el dashboard de impresiones.
public class ViewTempDtoCashInflowLista
{
    public int TransactionMasterId { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public decimal Monto { get; set; }
    public string MonedaSimbolo { get; set; } = string.Empty;
    public string MonedaNombre { get; set; } = string.Empty;
    public string Comentario { get; set; } = string.Empty;
    public string Referencia1 { get; set; } = string.Empty;
    public string Referencia2 { get; set; } = string.Empty;
}
