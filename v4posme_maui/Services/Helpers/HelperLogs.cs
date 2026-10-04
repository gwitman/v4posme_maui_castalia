using System.Diagnostics;
using System.Reflection;
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

    /// <summary>
    /// Recorre por reflexión todas las propiedades públicas de un objeto y registra, campo por
    /// campo, si su valor es NULL o cuál es su valor. Pensado para TbTransactionMaster y
    /// TbTransactionMasterDetail: permite ver exactamente qué campo quedó null antes de insertar.
    /// </summary>
    public static void DumpObject(string screen, string objectName, object? instance, string severity = "Info")
    {
        if (instance is null)
        {
            Write(severity, $"[{screen}] :: {objectName} = NULL (la instancia completa es null)");
            return;
        }

        try
        {
            var type = instance.GetType();
            Write(severity, $"[{screen}] :: DUMP {objectName} ({type.Name}) ---");
            foreach (var prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (!prop.CanRead) continue;
                object? value;
                try
                {
                    value = prop.GetValue(instance);
                }
                catch (Exception exProp)
                {
                    Write(severity, $"[{screen}] :: {objectName}.{prop.Name} = ERROR al leer ({exProp.Message})");
                    continue;
                }

                var estado = value is null ? "NULL" : "OK";
                var texto  = value is null ? "sin valor" : value.ToString();
                Write(severity, $"[{screen}] :: {objectName}.{prop.Name} = {estado} ({texto})");
            }
            Write(severity, $"[{screen}] :: FIN DUMP {objectName} ---");
        }
        catch (Exception e)
        {
            Debug.WriteLine($"HelperLogs.DumpObject error: {e.Message}");
        }
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
