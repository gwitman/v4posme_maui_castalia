using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace v4posme_maui.Services.Helpers;

/// <summary>
/// Utilidades para validar texto JSON recibido desde la API.
/// </summary>
public static class HelperJson
{
    /// <summary>
    /// Verifica que <paramref name="responseBody"/> sea un JSON valido (objeto o
    /// arreglo). Si no lo es, lanza una excepcion cuyo mensaje indica "Formato de
    /// JSON no valido" e incluye el texto crudo recibido para diagnostico.
    /// </summary>
    public static void ValidarJsonOrThrow(string? responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            throw new JsonException($"Formato de JSON no valido. Respuesta recibida: {responseBody}");
        }

        try
        {
            JToken.Parse(responseBody);
        }
        catch (JsonException)
        {
            throw new JsonException($"Formato de JSON no valido. Respuesta recibida: {responseBody}");
        }
    }
}
