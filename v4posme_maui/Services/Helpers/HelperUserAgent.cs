using System.Net.Http.Headers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.Services.Helpers;

/// <summary>
/// Resuelve y aplica el header User-Agent configurado en los parametros del
/// sistema (pantalla Parametros). Si el parametro no existe o esta vacio, se
/// usa <see cref="Constantes.DefaultUserAgent"/> (un valor tipo navegador).
/// Centraliza la logica para todos los puntos de descarga y subida a la API.
/// </summary>
public static class HelperUserAgent
{
    /// <summary>
    /// Obtiene el valor del User-Agent desde los parametros locales. Devuelve el
    /// valor por defecto si no esta configurado.
    /// </summary>
    public static async Task<string> GetUserAgentAsync()
    {
        try
        {
            var repository  = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
            var parameter   = await repository.PosMeFindByName(Constantes.ParametroUserAgent);
            if (parameter is not null && !string.IsNullOrWhiteSpace(parameter.Value))
            {
                return parameter.Value!;
            }
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
        }

        return Constantes.DefaultUserAgent;
    }

    /// <summary>
    /// Aplica el header User-Agent al request indicado, usando el valor
    /// configurado en parametros (o el valor por defecto).
    /// </summary>
    public static async Task ApplyUserAgentAsync(HttpRequestMessage request)
    {
        var userAgent = await GetUserAgentAsync();
        request.Headers.Remove("User-Agent");
        request.Headers.TryAddWithoutValidation("User-Agent", userAgent);
    }
}
