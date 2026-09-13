using System.Text.RegularExpressions;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.Services.Helpers;

/// <summary>
/// Genera codigos consecutivos con un prefijo fijo y 8 digitos.
/// El siguiente numero se calcula a partir del maximo ya existente en la base
/// de datos local para ese prefijo.
///  - Items:     prefijo "ITT" (ej. ITT00000001)
///  - Clientes:  prefijo "CLI" (ej. CLI00000001)
/// </summary>
public static class HelperConsecutivo
{
    public const string PrefijoItem     = "ITT";
    public const string PrefijoCustomer = "CLI";
    private const int CantidadDigitos   = 8;

    /// <summary>
    /// Devuelve el siguiente codigo de producto (ITT + 8 digitos) tomando como
    /// base el maximo consecutivo existente en los productos locales.
    /// </summary>
    public static async Task<string> PosMeSiguienteCodigoItem()
    {
        var repositorio = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        var items       = await repositorio.PosMeNameAsc();

        // Se consideran tanto ItemNumber como BarCode por si alguno trae el consecutivo.
        var maximo = 0;
        foreach (var item in items)
        {
            maximo = Math.Max(maximo, ExtraerConsecutivo(item.ItemNumber, PrefijoItem));
            maximo = Math.Max(maximo, ExtraerConsecutivo(item.BarCode, PrefijoItem));
        }

        return FormatearCodigo(PrefijoItem, maximo + 1);
    }

    /// <summary>
    /// Devuelve el siguiente codigo de cliente (CLI + 8 digitos) tomando como
    /// base el maximo consecutivo existente en los clientes locales.
    /// </summary>
    public static async Task<string> PosMeSiguienteCodigoCustomer()
    {
        var repositorio = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        var customers   = await repositorio.PosMeAscTake10(int.MaxValue);

        var maximo = 0;
        foreach (var customer in customers)
        {
            maximo = Math.Max(maximo, ExtraerConsecutivo(customer.CustomerNumber, PrefijoCustomer));
            maximo = Math.Max(maximo, ExtraerConsecutivo(customer.Identification, PrefijoCustomer));
        }

        return FormatearCodigo(PrefijoCustomer, maximo + 1);
    }

    /// <summary>
    /// Extrae la parte numerica de un codigo que empieza con el prefijo indicado.
    /// Devuelve 0 si el codigo es nulo, no empieza con el prefijo o no es numerico.
    /// </summary>
    private static int ExtraerConsecutivo(string? codigo, string prefijo)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return 0;

        codigo = codigo.Trim();
        if (!codigo.StartsWith(prefijo, StringComparison.OrdinalIgnoreCase))
            return 0;

        var parteNumerica = codigo[prefijo.Length..];
        var soloDigitos   = Regex.Match(parteNumerica, @"^\d+").Value;
        return int.TryParse(soloDigitos, out var valor) ? valor : 0;
    }

    private static string FormatearCodigo(string prefijo, int numero)
    {
        return $"{prefijo}{numero.ToString().PadLeft(CantidadDigitos, '0')}";
    }
}
