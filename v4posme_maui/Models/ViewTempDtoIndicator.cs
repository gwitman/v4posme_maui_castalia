namespace v4posme_maui.Models;

/// <summary>
/// Modelo de presentacion para mostrar un indicador en la tarjeta del dashboard.
/// Arma el texto final combinando prefijo + valor + sufijo.
/// </summary>
public class ViewTempDtoIndicator
{
    public string? Name { get; set; }

    public string? SystemName { get; set; }

    public decimal Value { get; set; }

    public int Order { get; set; }

    public string? Prefix { get; set; }

    public string? Posfix { get; set; }

    // Texto formateado: prefijo + valor (miles con 2 decimales) + sufijo.
    public string ValorFormateado => $"{Prefix}{Value:N2}{Posfix}".Trim();
}
