namespace v4posme_maui.Models;

// Estado temporal de la salida recien registrada, usado para mostrar el comprobante
// en la pantalla de resultado (patron similar a ViewTempDtoCashInflow).
public class ViewTempDtoCashOutflow
{
    public int TransactionMasterId { get; set; }
    public string NumeroSalida { get; set; } = string.Empty;
    public DateTime Fecha { get; set; } = DateTime.Now;
    public string MonedaNombre { get; set; } = string.Empty;
    public string MonedaSimbolo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string Comentario { get; set; } = string.Empty;
    public string Referencia1 { get; set; } = string.Empty;
    public string Referencia2 { get; set; } = string.Empty;

    public string MontoFormateado => $"{MonedaSimbolo} {Monto:N2}";
}
