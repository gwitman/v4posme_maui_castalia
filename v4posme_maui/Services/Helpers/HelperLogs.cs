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
