using System.Diagnostics;
using Unity;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Helpers;

/// <summary>
/// Helper estático para registrar excepciones en la tabla tb_logs.
/// Se usa dentro de los bloques try/catch del sistema.
/// </summary>
public static class HelperLogs
{
    /// <summary>
    /// Registra una excepción en la tabla de logs. No lanza excepciones (fire-and-forget).
    /// </summary>
    public static void Log(Exception exception, string severity = "Error")
    {
        Write(severity, exception.ToString());
    }

    /// <summary>
    /// Registra un mensaje en la tabla de logs. No lanza excepciones (fire-and-forget).
    /// </summary>
    public static void Log(string message, string severity = "Error")
    {
        Write(severity, message);
    }

    /// <summary>
    /// Registra un mensaje con contexto de pantalla/metodo (traza de seguimiento).
    /// Ejemplo de salida: [PaymentInvoice] -> OnAplicarPagoCommand :: inicio
    /// </summary>
    public static void Trace(string screen, string step, string? detail = null, string severity = "Info")
    {
        var message = string.IsNullOrWhiteSpace(detail)
            ? $"[{screen}] -> {step}"
            : $"[{screen}] -> {step} :: {detail}";
        Write(severity, message);
    }

    /// <summary>
    /// Registra el estado de un valor (null / no null) para rastrear referencias nulas.
    /// </summary>
    public static void TraceValue(string screen, string field, object? value, string severity = "Info")
    {
        var estado = value is null ? "NULL" : "OK";
        Write(severity, $"[{screen}] :: {field} = {estado} ({(value is null ? "sin valor" : value.ToString())})");
    }

    private static void Write(string severity, string logs)
    {
        try
        {
            var repository = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbLogs>();
            _ = repository.PosMeInsertLog(severity, logs);
        }
        catch (Exception e)
        {
            // No propagar errores de logging
            Debug.WriteLine($"HelperLogs error: {e.Message}");
        }
    }
}
