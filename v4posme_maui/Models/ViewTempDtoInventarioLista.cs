namespace v4posme_maui.Models;

// Fila para el listado de inventario (Entradas / Salidas) en la pantalla de impresiones.
// Muestra fecha, codigo, cantidad de productos, comentario y costo total.
public class ViewTempDtoInventarioLista
{
    public int TransactionMasterId { get; set; }

    public string Codigo { get; set; } = string.Empty;

    public DateTime Fecha { get; set; }

    public int CantidadProductos { get; set; }

    public string Comentario { get; set; } = string.Empty;

    public string Referencia1 { get; set; } = string.Empty;

    public string Referencia2 { get; set; } = string.Empty;

    public decimal CostoTotal { get; set; }

    public string MonedaSimbolo { get; set; } = "C$";

    public string FechaTexto => Fecha.ToString("dd/MM/yyyy HH:mm");

    public string CostoTotalTexto => $"{MonedaSimbolo} {CostoTotal:N2}";
}
