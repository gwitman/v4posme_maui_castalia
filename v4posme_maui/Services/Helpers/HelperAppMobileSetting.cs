using System.Globalization;
using Newtonsoft.Json.Linq;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Helpers;

/// <summary>
/// Interpreta el parámetro de servidor APP_MOBILE_SETTING (JSON) que configura los campos
/// de la factura móvil (pantalla 2, comprobante 8, voucher e impresión).
/// Clase pura (sin dependencias de MAUI) para poder probarla en v4posme_maui.Tests.
///
/// Formato: un objeto con todas las llaves (o un arreglo de objetos), y cada llave con la
/// misma estructura {visible, required, value_default, label}. El título del documento se
/// toma del "label" de label_type_document_in_register / label_type_document_in_aplied:
/// {"control_zone_facturacion":{"visible":true,"required":false,"value_default":"3","label":"Zona"},
///  "label_type_document_in_register":{"visible":true,"required":false,"value_default":"","label":"PROFORMA"},
///  "label_type_document_in_aplied":{"visible":true,"required":false,"value_default":"","label":"FACTURA"}}
/// </summary>
public static class HelperAppMobileSetting
{
    public static DtoAppMobileSetting Parse(string? json)
    {
        var setting = new DtoAppMobileSetting();
        if (string.IsNullOrWhiteSpace(json))
            return setting;

        try
        {
            var token = JToken.Parse(json);
            if (token is JArray arreglo)
            {
                foreach (var elemento in arreglo.OfType<JObject>())
                    LeerObjeto(elemento, setting);
            }
            else if (token is JObject objeto)
            {
                LeerObjeto(objeto, setting);
            }
            else
            {
                setting.Error = "APP_MOBILE_SETTING no es un objeto ni un arreglo JSON.";
            }
        }
        catch (Exception ex)
        {
            // JSON mal formado: se ignora la configuración (comportamiento por defecto) y se informa el error.
            setting.Controles.Clear();
            setting.Error = $"APP_MOBILE_SETTING inválido: {ex.Message}";
        }

        return setting;
    }

    private static void LeerObjeto(JObject objeto, DtoAppMobileSetting setting)
    {
        foreach (var propiedad in objeto.Properties())
        {
            var nombre = propiedad.Name.Trim();
            if (propiedad.Value is JObject control)
            {
                setting.Controles[nombre] = new DtoAppMobileSettingControl
                {
                    Visible      = LeerBool(control, "visible"),
                    Required     = LeerBool(control, "required"),
                    ValueDefault = LeerTexto(control, "value_default"),
                    Label        = LeerTexto(control, "label")
                };
                continue;
            }

            // Tolerancia: si una llave viene como texto simple ("llave":"valor") se toma como su label.
            var texto = TextoDeToken(propiedad.Value);
            if (texto is not null && propiedad.Value.Type != JTokenType.Array)
                setting.Controles[nombre] = new DtoAppMobileSettingControl { Label = texto };
        }
    }

    private static JToken? Buscar(JObject objeto, string nombre)
        => objeto.Properties().FirstOrDefault(p => string.Equals(p.Name.Trim(), nombre, StringComparison.OrdinalIgnoreCase))?.Value;

    private static string? LeerTexto(JObject objeto, string nombre) => TextoDeToken(Buscar(objeto, nombre));

    private static string? TextoDeToken(JToken? token)
    {
        if (token is null || token.Type == JTokenType.Null)
            return null;
        var texto = token.Type == JTokenType.String ? token.Value<string>() : token.ToString();
        return string.IsNullOrWhiteSpace(texto) ? null : texto.Trim();
    }

    // Acepta true/false, "true"/"false", "1"/"0", "si"/"no" y números.
    private static bool? LeerBool(JObject objeto, string nombre)
    {
        var token = Buscar(objeto, nombre);
        if (token is null || token.Type == JTokenType.Null)
            return null;
        switch (token.Type)
        {
            case JTokenType.Boolean:
                return token.Value<bool>();
            case JTokenType.Integer:
                return token.Value<long>() != 0;
        }

        var texto = token.ToString().Trim().ToLowerInvariant();
        return texto switch
        {
            "true" or "1" or "si" or "sí" or "yes" => true,
            "false" or "0" or "no"                 => false,
            _                                      => null
        };
    }

    // ===== Consultas =====

    /// <summary>El control está definido en el JSON.</summary>
    public static bool Existe(DtoAppMobileSetting setting, string clave) => setting.GetControl(clave) is not null;

    /// <summary>Visible según el JSON; si no se define se usa <paramref name="porDefecto"/>.</summary>
    public static bool EsVisible(DtoAppMobileSetting setting, string clave, bool porDefecto = true)
        => setting.GetControl(clave)?.Visible ?? porDefecto;

    /// <summary>
    /// Obligatorio solo si el JSON lo indica y el control está visible: un campo oculto no
    /// puede ser completado por el usuario (se llena con su valor por defecto).
    /// </summary>
    public static bool EsObligatorio(DtoAppMobileSetting setting, string clave)
        => (setting.GetControl(clave)?.Required ?? false) && EsVisible(setting, clave);

    public static string Etiqueta(DtoAppMobileSetting setting, string clave, string porDefecto)
        => setting.GetControl(clave)?.Label ?? porDefecto;

    public static string? ValorPorDefecto(DtoAppMobileSetting setting, string clave)
        => setting.GetControl(clave)?.ValueDefault;

    /// <summary>
    /// Mesa obligatoria: si el JSON define el control de mesa manda el JSON (visible y required);
    /// si no lo define se conserva la regla anterior (obligatoria solo en modo restaurante).
    /// </summary>
    public static bool MesaObligatoria(DtoAppMobileSetting setting, bool esModoRestaurante)
        => Existe(setting, Constantes.SettingControlMesa)
            ? EsObligatorio(setting, Constantes.SettingControlMesa)
            : esModoRestaurante;

    /// <summary>
    /// Mesa visible en comprobante/impresión: JSON si lo define; si no, regla anterior (modo restaurante).
    /// </summary>
    public static bool MesaVisibleEnComprobante(DtoAppMobileSetting setting, bool esModoRestaurante)
        => EsVisible(setting, Constantes.SettingControlMesa, esModoRestaurante);

    /// <summary>
    /// Busca la opción de un combo por ID (Key) o por nombre/símbolo (sin distinguir
    /// mayúsculas ni tildes). Devuelve null si no hay coincidencia.
    /// </summary>
    public static DtoCatalogItem? BuscarOpcion(IEnumerable<DtoCatalogItem>? opciones, string? valor)
    {
        if (opciones is null || string.IsNullOrWhiteSpace(valor))
            return null;
        var buscado = valor.Trim();
        var lista   = opciones.ToList();

        if (int.TryParse(buscado, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id))
        {
            var porId = lista.FirstOrDefault(o => o.Key == id);
            if (porId is not null)
                return porId;
        }

        return lista.FirstOrDefault(o => TextoIgual(o.Name, buscado))
            ?? lista.FirstOrDefault(o => TextoIgual(o.Simbolo, buscado));
    }

    private static bool TextoIgual(string? a, string b)
        => a is not null && string.Compare(a.Trim(), b, CultureInfo.InvariantCulture,
               CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace) == 0;

    /// <summary>
    /// Título del documento: registrada → label_type_document_in_register; aplicada/anulada →
    /// label_type_document_in_aplied. Por defecto "FACTURA".
    /// </summary>
    public static string TituloDocumento(DtoAppMobileSetting setting, int statusId)
    {
        var clave = statusId == (int)TypeStatusBilling.Register
            ? Constantes.SettingLabelTypeDocumentInRegister
            : Constantes.SettingLabelTypeDocumentInAplied;
        return Etiqueta(setting, clave, Mensajes.TituloDocumentoFactura);
    }

    /// <summary>
    /// Devuelve el rótulo del primer campo obligatorio (según el JSON) que no tiene valor, o
    /// null si todo está completo. La mesa se valida aparte con <see cref="MesaObligatoria"/>.
    /// Los combos sin selección tienen Key &lt;= 0 ("Seleccione").
    /// </summary>
    public static string? CampoObligatorioFaltante(
        DtoAppMobileSetting setting,
        int monedaKey,
        int tipoDocumentoKey,
        int zonaKey,
        string? telefono,
        string? cedula,
        string? clienteReferencia,
        string etiquetaZonaPorDefecto)
    {
        var campos = new (string Clave, string Etiqueta, bool TieneValor)[]
        {
            (Constantes.SettingControlClienteReferencia, Mensajes.EtiquetaClienteReferenciaFactura, !string.IsNullOrWhiteSpace(clienteReferencia)),
            (Constantes.SettingControlCedula,            Mensajes.EtiquetaCedulaFactura,            !string.IsNullOrWhiteSpace(cedula)),
            (Constantes.SettingControlTelefono,          Mensajes.EtiquetaTelefonoFactura,          !string.IsNullOrWhiteSpace(telefono)),
            (Constantes.SettingControlMoneda,            Mensajes.EtiquetaMonedaFactura,            monedaKey > 0),
            (Constantes.SettingControlTipoDocumento,     Mensajes.EtiquetaTipoDocumentoFactura,     tipoDocumentoKey > 0),
            (Constantes.SettingControlZona,              etiquetaZonaPorDefecto,                    zonaKey > 0)
        };

        foreach (var campo in campos)
        {
            if (EsObligatorio(setting, campo.Clave) && !campo.TieneValor)
                return Etiqueta(setting, campo.Clave, campo.Etiqueta);
        }
        return null;
    }

    /// <summary>
    /// Líneas de datos de la factura para el comprobante (pantalla 8 / voucher) y la impresión,
    /// en este orden: Tipo, Moneda, Mesa, Zona, Teléfono, Cédula, Cliente. Tipo y Moneda se
    /// muestran siempre que estén visibles (comportamiento previo); el resto solo si tiene valor.
    /// </summary>
    public static List<DtoLineaComprobante> LineasComprobante(
        DtoAppMobileSetting setting,
        string? tipoDocumento,
        string? moneda,
        string? mesa,
        bool esModoRestaurante,
        string? zona,
        string etiquetaZonaPorDefecto,
        string? telefono,
        string? cedula,
        string? clienteReferencia)
    {
        var lineas = new List<DtoLineaComprobante>();

        void Agregar(string clave, string etiquetaPorDefecto, string? valor, bool visible, bool soloConValor)
        {
            if (!visible || (soloConValor && string.IsNullOrWhiteSpace(valor)))
                return;
            lineas.Add(new DtoLineaComprobante(clave, Etiqueta(setting, clave, etiquetaPorDefecto), valor?.Trim() ?? string.Empty));
        }

        Agregar(Constantes.SettingControlTipoDocumento,     Mensajes.EtiquetaTipoComprobante,          tipoDocumento,     EsVisible(setting, Constantes.SettingControlTipoDocumento), false);
        Agregar(Constantes.SettingControlMoneda,            Mensajes.EtiquetaMonedaFactura,            moneda,            EsVisible(setting, Constantes.SettingControlMoneda), false);
        Agregar(Constantes.SettingControlMesa,              Mensajes.EtiquetaMesaFactura,              mesa,              MesaVisibleEnComprobante(setting, esModoRestaurante), true);
        Agregar(Constantes.SettingControlZona,              etiquetaZonaPorDefecto,                    zona,              EsVisible(setting, Constantes.SettingControlZona), true);
        Agregar(Constantes.SettingControlTelefono,          Mensajes.EtiquetaTelefonoFactura,          telefono,          EsVisible(setting, Constantes.SettingControlTelefono), true);
        Agregar(Constantes.SettingControlCedula,            Mensajes.EtiquetaCedulaFactura,            cedula,            EsVisible(setting, Constantes.SettingControlCedula), true);
        Agregar(Constantes.SettingControlClienteReferencia, Mensajes.EtiquetaClienteReferenciaFactura, clienteReferencia, EsVisible(setting, Constantes.SettingControlClienteReferencia), true);
        return lineas;
    }
}
