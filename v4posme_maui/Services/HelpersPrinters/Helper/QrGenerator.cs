using v4posme_maui.Models;

namespace v4posme_maui.Services.HelpersPrinters.Helper;

public static class QrGenerator
{
    private const int MaxLength = 255;

    public static string BuildContent(ViewTempDtoInvoice invoice)
    {
        var codigo = string.IsNullOrWhiteSpace(invoice.Codigo)
            ? "SIN-CODIGO"
            : invoice.Codigo;

        var cliente = string.IsNullOrWhiteSpace(invoice.NombreCompleto)
            ? "CLIENTE-DESCONOCIDO"
            : invoice.NombreCompleto;

        var fecha = invoice.TransactionOn.ToString("yyyy-MM-dd HH:mm");
        var content = $"FAC:{codigo}|CLI:{cliente}|TOT:{invoice.Balance:N2}|FEC:{fecha}";

        return content.Length > MaxLength
            ? content[..MaxLength]
            : content;
    }
}
