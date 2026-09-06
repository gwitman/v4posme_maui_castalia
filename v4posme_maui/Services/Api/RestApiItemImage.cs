using System.Net.Http.Headers;
using Newtonsoft.Json;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Api;

/// <summary>
/// Cliente REST para descargar y subir la imagen de un producto (item).
/// - Descarga: pide al servidor la imagen del producto por su itemID.
/// - Subida: envia al servidor la imagen seleccionada por el usuario.
/// Todas las operaciones son asincronas para no bloquear la UI mientras se
/// espera la respuesta del servidor.
/// </summary>
public class RestApiItemImage
{
    private readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Descarga la imagen del producto indicado. Devuelve los bytes de la imagen
    /// o null si no existe, hay error o el servidor no responde. El llamador debe
    /// mostrar una imagen por defecto cuando el resultado sea null.
    /// </summary>
    public async Task<byte[]?> GetImageAsync(int itemId)
    {
        try
        {
            var tempUrl = Constantes.UrlGetUploadImageItem.Replace("{UrlBase}", VariablesGlobales.CompanyKey);

            var nvc = new List<KeyValuePair<string, string>>
            {
                new("txtNickname", VariablesGlobales.User?.Nickname ?? string.Empty),
                new("txtPassword", VariablesGlobales.User?.Password ?? string.Empty),
                new("txtCompanyID", Constantes.CompanyId.ToString()),
                new("txtItemID", itemId.ToString())
            };

            var req = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = new FormUrlEncodedContent(nvc)
            };

            var response = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode)
            {
                HelperLogs.Log($"GetImageAsync: respuesta HTTP no exitosa ({(int)response.StatusCode}) para itemID={itemId}", "Warning");
                return null;
            }

            var contentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

            // El servidor puede responder con la imagen binaria directamente o con un
            // JSON que contiene la imagen en base64. Soportamos ambos casos.
            if (contentType.Contains("image", StringComparison.OrdinalIgnoreCase))
            {
                return await response.Content.ReadAsByteArrayAsync();
            }

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return null;

            // Intentar interpretar como JSON { "error": bool, "image": "base64..." }.
            try
            {
                var dto = JsonConvert.DeserializeObject<ItemImageResponse>(body);
                if (dto is null || dto.Error || string.IsNullOrWhiteSpace(dto.Image))
                    return null;

                var base64 = dto.Image.Contains(',')
                    ? dto.Image[(dto.Image.IndexOf(',') + 1)..]
                    : dto.Image;
                return Convert.FromBase64String(base64);
            }
            catch (JsonException)
            {
                // El cuerpo no es JSON: intentar interpretarlo como base64 plano.
                try
                {
                    var base64 = body.Contains(',') ? body[(body.IndexOf(',') + 1)..] : body;
                    return Convert.FromBase64String(base64.Trim());
                }
                catch (FormatException)
                {
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
            HelperLogs.Log($"GetImageAsync: excepcion al descargar imagen del itemID={itemId}", "Error");
            return null;
        }
    }

    /// <summary>
    /// Sube al servidor la imagen del producto indicado. Devuelve true si la
    /// operacion fue exitosa.
    /// </summary>
    public async Task<bool> UploadImageAsync(int itemId, byte[] imageBytes, string fileName = "item.jpg")
    {
        try
        {
            if (imageBytes is null || imageBytes.Length == 0)
                return false;

            var tempUrl = Constantes.UrlSetUploadImageItem.Replace("{UrlBase}", VariablesGlobales.CompanyKey);

            using var content = new MultipartFormDataContent
            {
                { new StringContent(VariablesGlobales.User?.Nickname ?? string.Empty), "txtNickname" },
                { new StringContent(VariablesGlobales.User?.Password ?? string.Empty), "txtPassword" },
                { new StringContent(Constantes.CompanyId.ToString()), "txtCompanyID" },
                { new StringContent(itemId.ToString()), "txtItemID" }
            };

            var imageContent = new ByteArrayContent(imageBytes);
            imageContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
            content.Add(imageContent, "txtImage", fileName);

            var req = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = content
            };

            var response = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode)
            {
                HelperLogs.Log($"UploadImageAsync: respuesta HTTP no exitosa ({(int)response.StatusCode}) para itemID={itemId}", "Error");
                return false;
            }

            var body = await response.Content.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(body))
                return true;

            try
            {
                var dto = JsonConvert.DeserializeObject<ItemImageResponse>(body);
                return dto is null || !dto.Error;
            }
            catch (JsonException)
            {
                return true;
            }
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
            HelperLogs.Log($"UploadImageAsync: excepcion al subir imagen del itemID={itemId}", "Error");
            return false;
        }
    }

    /// <summary>
    /// Verifica que los bytes correspondan a un formato de imagen soportado leyendo su
    /// "firma magica" (magic number). Sirve para confirmar que el servidor PHP envio el
    /// binario correcto y no un texto/HTML/JSON o un binario corrupto.
    /// - JPEG:  FF D8 FF
    /// - PNG:   89 50 4E 47 0D 0A 1A 0A
    /// - GIF:   47 49 46 38 ("GIF8")
    /// - WEBP:  "RIFF"...."WEBP"
    /// Registra en el log el detalle para diagnostico.
    /// </summary>
    public static bool EsImagenValida(byte[]? bytes)
    {
        if (bytes is null || bytes.Length < 12)
        {
            HelperLogs.Log($"EsImagenValida: bytes nulos o muy cortos (len={bytes?.Length ?? 0})", "Warning");
            return false;
        }

        bool esJpeg = bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        bool esPng  = bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;
        bool esGif  = bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x38;
        bool esWebp = bytes[0] == 0x52 && bytes[1] == 0x49 && bytes[2] == 0x46 && bytes[3] == 0x46
                      && bytes[8] == 0x57 && bytes[9] == 0x45 && bytes[10] == 0x42 && bytes[11] == 0x50;

        var esValida = esJpeg || esPng || esGif || esWebp;
        if (!esValida)
        {
            // Loguear los primeros bytes en hex para ver que devolvio realmente el servidor.
            var primeros = BitConverter.ToString(bytes, 0, Math.Min(16, bytes.Length));
            HelperLogs.Log($"EsImagenValida: firma no reconocida. Primeros bytes: {primeros}", "Error");
        }

        return esValida;
    }

    private sealed class ItemImageResponse
    {
        [JsonProperty("error")]
        public bool Error { get; set; }

        [JsonProperty("image")]
        public string? Image { get; set; }

        [JsonProperty("message")]
        public string? Message { get; set; }
    }
}
