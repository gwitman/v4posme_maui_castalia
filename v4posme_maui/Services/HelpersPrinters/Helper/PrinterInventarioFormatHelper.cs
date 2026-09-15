using v4posme_maui.Models;

namespace v4posme_maui.Services.HelpersPrinters.Helper;

// Formato de impresion para las transacciones de inventario (Compras / Otras salidas).
public static class PrinterInventarioFormatHelper
{
    public static Task PrintFormat(
        Printer printer,
        ViewTempDtoInventario dto,
        string titulo,
        string companyName,
        string userNickname)
    {
        printer.AlignCenter();
        if (!string.IsNullOrWhiteSpace(companyName))
            printer.BoldMode(companyName);
        printer.BoldMode(titulo);
        printer.BoldMode(dto.Codigo);
        printer.BoldMode($"FECHA: {dto.TransactionOn:yyyy-MM-dd hh:mm tt}");
        printer.NewLine();
        printer.AlignLeft();

        var header = $"""
                      USUARIO      :{userNickname}
                      COMENTARIO   :{dto.Comentarios}
                      REFERENCIA1  :{dto.Referencia1}
                      REFERENCIA2  :{dto.Referencia2}
                      """;
        printer.Append(header);
        printer.NewLine();

        printer.Append("CANT.    PRECIO       TOTAL");
        foreach (var item in dto.Items)
        {
            printer.Separator();
            printer.Append(item.Name);
            var cant   = item.Quantity.ToString("N2").PadLeft(6);
            var precio = item.PrecioPublico.ToString("N2").PadLeft(10);
            var total  = (item.PrecioPublico * item.Quantity).ToString("N2").PadLeft(10);
            printer.Append($"{cant}  {precio}  {total}");
        }
        printer.Separator();
        printer.NewLine();

        const int labelWidth = 12;
        const int valueWidth = 12;
        var totalGeneral = dto.Items.Sum(p => p.PrecioPublico * p.Quantity);
        printer.Append($"{"TOTAL:".PadRight(labelWidth)}{totalGeneral.ToString("N2").PadLeft(valueWidth)}");
        printer.NewLine();
        printer.AlignCenter();
        printer.Append("posMe");
        printer.NewLine();
        printer.FullPaperCut();

        return Task.CompletedTask;
    }
}
