using Newtonsoft.Json.Linq;
using v4posme_maui.Models;

namespace v4posme_maui.Services.Helpers;

/// <summary>
/// Logica pura de "Cargar solicitud" (sin MAUI/SQLite) para poder probarla en v4posme_maui.Tests.
/// </summary>
public static class HelperLoadOrder
{
    public static string NormalizarNumero(string? orderNumber)
    {
        return (orderNumber ?? string.Empty).Trim();
    }

    /// <summary>
    /// Interpreta la respuesta JSON del servidor. Si hay error el servidor envia "data": [] (arreglo),
    /// por eso no se deserializa directo a la clase. Lanza excepcion si el texto no es JSON valido.
    /// </summary>
    public static DtoLoadOrderResponse ParsearRespuesta(string? responseBody, string mensajeErrorGenerico)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
            return new DtoLoadOrderResponse { Error = true, Message = mensajeErrorGenerico };

        var json      = JObject.Parse(responseBody);
        var error     = json["error"]?.Type == JTokenType.Boolean ? json["error"]!.Value<bool>() : true;
        var message   = json["message"]?.ToString() ?? string.Empty;
        var resultado = new DtoLoadOrderResponse { Error = error, Message = message };

        if (error)
        {
            if (string.IsNullOrWhiteSpace(resultado.Message))
                resultado.Message = mensajeErrorGenerico;
            return resultado;
        }

        if (json["data"] is not JObject data)
        {
            resultado.Error   = true;
            resultado.Message = mensajeErrorGenerico;
            return resultado;
        }

        resultado.Data       = data.ToObject<DtoLoadOrderData>() ?? new DtoLoadOrderData();
        resultado.Data.Items ??= [];
        return resultado;
    }

    /// <summary>
    /// Agrupa las lineas de la solicitud por producto sumando cantidades, conservando el orden
    /// de aparicion (la web agrega cada linea con "sumar = true").
    /// </summary>
    public static List<DtoLoadOrderItem> ConsolidarItems(IEnumerable<DtoLoadOrderItem?>? items)
    {
        var resultado = new List<DtoLoadOrderItem>();
        if (items is null) return resultado;

        foreach (var item in items)
        {
            if (item is null) continue;
            var existente = resultado.FirstOrDefault(r => r.ItemID == item.ItemID);
            if (existente is null)
                resultado.Add(new DtoLoadOrderItem { ItemID = item.ItemID, Quantity = item.Quantity, Price = item.Price });
            else
                existente.Quantity += item.Quantity;
        }
        return resultado;
    }

    /// <summary>
    /// Valor a usar al cargar la solicitud. Igual que la web (fnCargarSolicitudAjax), el dato de la
    /// orden SIEMPRE reemplaza lo que haya en pantalla, sin importar el cliente elegido en el paso 1
    /// (generico u otro); si el servidor no trae dato el campo queda vacio.
    /// </summary>
    public static string ValorSolicitud(string? valorServidor)
    {
        return valorServidor?.Trim() ?? string.Empty;
    }
}
