namespace v4posme_maui.Models;

/// <summary>
/// Configuración de un control de la factura móvil leída del parámetro de servidor
/// APP_MOBILE_SETTING. Todas las propiedades son opcionales en el JSON: lo que no venga
/// conserva el comportamiento actual de la app (visible, no obligatorio, sin valor por
/// defecto y rótulo original).
/// </summary>
public class DtoAppMobileSettingControl
{
    public bool? Visible { get; set; }
    public bool? Required { get; set; }
    public string? ValueDefault { get; set; }
    public string? Label { get; set; }
}

/// <summary>
/// Resultado de interpretar el JSON de APP_MOBILE_SETTING.
/// </summary>
public class DtoAppMobileSetting
{
    // Todas las llaves del JSON (control_*_facturacion y label_type_document_in_register /
    // label_type_document_in_aplied) tienen la misma forma {visible, required, value_default, label}.
    // Los títulos del documento se leen del "label" de su llave.
    // Las llaves se comparan sin distinguir mayúsculas para tolerar errores de captura en el servidor.
    public Dictionary<string, DtoAppMobileSettingControl> Controles { get; } = new(StringComparer.OrdinalIgnoreCase);

    // Mensaje de error si el JSON no se pudo interpretar (la app usa entonces el comportamiento por defecto).
    public string? Error { get; set; }

    public DtoAppMobileSettingControl? GetControl(string clave)
        => Controles.TryGetValue(clave, out var control) ? control : null;
}

/// <summary>
/// Línea "Etiqueta: Valor" que se muestra en el comprobante (pantalla 8 / voucher) y se imprime.
/// </summary>
public record DtoLineaComprobante(string Clave, string Etiqueta, string Valor)
{
    public string EtiquetaConDosPuntos => $"{Etiqueta}:";
}
