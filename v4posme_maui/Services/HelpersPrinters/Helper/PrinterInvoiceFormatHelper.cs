using SkiaSharp;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.HelpersPrinters.Enums;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.HelpersPrinters.Helper;

public static class PrinterInvoiceFormatHelper
{
    /// <summary>
    /// Aplica el formato "default" al printer: logo, encabezado, items, totales, QR y pie de página.
    /// Equivale al bloque original desde logo hasta printer.Print().
    /// </summary>
    public static async Task PrintDefaultFormat(
        Printer printer,
        ViewTempDtoInvoice dtoInvoice,
        TbParameterSystem logo,
        string companyName,
        string companyRuc,
        string companyAddress,
        string companyTelefono,
        string printerQR,
        string printerQRUrl,
        string printerReferencia,
        string userNickname,
        HelperCore helperCore)
    {
        if (!string.IsNullOrWhiteSpace(logo.Value))
        {
            var readImage = Convert.FromBase64String(logo.Value!);
            printer.AlignRight();
            printer.Image(SKBitmap.Decode(readImage));
        }

        printer.AlignCenter();
        printer.BoldMode(companyName);
        printer.BoldMode($"RUC: {companyRuc}");
        printer.BoldMode("FACTURA");
        printer.BoldMode(dtoInvoice.Codigo);
        printer.BoldMode($"FECHA: {dtoInvoice.TransactionOn:yyyy-MM-dd hh:mm tt}");
        printer.NewLine();
        printer.AlignLeft();

        var detalleHeader = $"""
                             VENDEDOR     :{userNickname}
                             CODIGO       :{dtoInvoice.CustomerResponse.CustomerNumber}
                             MONEDA       :{dtoInvoice.Currency!.Simbolo}
                             TIPO         :{dtoInvoice.TipoDocumento!.Name}
                             CLIENTE       
                             {dtoInvoice.NombreCompleto}
                             {dtoInvoice.Comentarios}
                             """;
        printer.NewLine();
        printer.Append(detalleHeader);
        printer.NewLine();

        bool showReferencia = printerReferencia.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);

        printer.Append("CANT.    PRECIO       TOTAL");
        foreach (var item in dtoInvoice.Items)
        {
            printer.Separator();
            printer.Append(item.Name);
            var cant   = item.Quantity.ToString("N2").PadLeft(6);
            var precio = item.PrecioPublico.ToString("N2").PadLeft(10);
            var total  = item.Importe.ToString("N2").PadLeft(10);
            printer.Append($"{cant}  {precio}  {total}");
            if (showReferencia && !string.IsNullOrWhiteSpace(item.Referencia))
            {
                printer.Append($"REF: {item.Referencia}");
            }
        }
        printer.Separator();

        printer.NewLine();
        const int labelWidth = 12;
        const int valueWidth = 12;
        printer.Append($"{"SUB TOTAL:".PadRight(labelWidth)}{dtoInvoice.TransactionMaster.Amount.ToString("N2").PadLeft(valueWidth)}");
        printer.Append($"{"DESCUENTO:".PadRight(labelWidth)}{dtoInvoice.TransactionMaster.Discount.ToString("N2").PadLeft(valueWidth)}");
        printer.Append($"{"TOTAL:".PadRight(labelWidth)}{(dtoInvoice.TransactionMaster.SubAmount - dtoInvoice.TransactionMaster.Discount).ToString("N2").PadLeft(valueWidth)}");
        printer.Append($"{"RECIBIDO:".PadRight(labelWidth)}{dtoInvoice.Monto.ToString("N2").PadLeft(valueWidth)}");
        printer.Append($"{"CAMBIO:".PadRight(labelWidth)}{dtoInvoice.Cambio.ToString("N2").PadLeft(valueWidth)}");
        printer.NewLine();
        printer.AlignCenter();

        if (printerQR.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            var qrContent = printerQRUrl + "/inm/" + dtoInvoice.Codigo + "/unm/" + userNickname;
            printer.QrCode(qrContent, QrCodeSize.Size1);
            printer.NewLine();
            printer.NewLine();
        }

        printer.Append(companyAddress);
        printer.Append(companyTelefono);
        printer.NewLine();
        printer.NewLine();

        printer.FullPaperCut();
        printer.Print();

        await Task.CompletedTask;
    }
    public static async Task PrintLotoFormat(
        Printer printer,
        ViewTempDtoInvoice dtoInvoice,
        TbParameterSystem logo,
        string companyName,
        string companyRuc,
        string companyAddress,
        string companyTelefono,
        string printerQR,
        string printerQRUrl,
        string printerReferencia,
        string userNickname,
        HelperCore helperCore)
    {
        string ocultarRucRaw = await helperCore.GetValueParameter("MOBILE_IN_PRINTER_INVOICE_OCULTAR_RUC", "false");
        bool ocultarRuc      = ocultarRucRaw.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);


        //Mostrar Imagen
        if (!string.IsNullOrWhiteSpace(logo.Value))
        {
            var readImage = Convert.FromBase64String(logo.Value!);
            printer.AlignRight();
            printer.Image(SKBitmap.Decode(readImage));
        }

        //Mostrar Ruc
        printer.AlignCenter();
        printer.BoldMode(companyName);

        //Calculo de tiempo
        var txHour = dtoInvoice.TransactionOn.TimeOfDay;
        string tiempo;
        if (txHour < TimeSpan.FromHours(12))
            tiempo = "MAÑANA";
        else if (txHour < TimeSpan.FromHours(15))
            tiempo = "TARDE";
        else if (txHour < TimeSpan.FromHours(18))
            tiempo = "06:00 P.M";
        else
            tiempo = "NOCHE";

        //printer.NewLine();
        printer.AlignLeft();
        var detalleHeader = $"""
                             FACTURA  :{dtoInvoice.Codigo}
                             FECHA    :{dtoInvoice.TransactionOn:yyyy-MM-dd hh:mm tt}
                             VENDEDOR :{userNickname}
                             CLIENTE  :{dtoInvoice.Comentarios}
                             TIEMPO   :{tiempo}
                             """;
        printer.NewLine();
        printer.Append(detalleHeader);
        //printer.NewLine();

        bool showReferencia = printerReferencia.Trim().Equals("true", StringComparison.OrdinalIgnoreCase);

        printer.Append("NUM.    MONTO       PREMIO");
        foreach (var item in dtoInvoice.Items)
        {
            //printer.Separator('-', 40);
            printer.Append(item.Name);
            var cant            = item.Quantity.ToString("N2").PadLeft(6);
            var precio          = ("C$ " + item.PrecioPublico.ToString("N0")).PadLeft(7);
            var total           = ("C$ " + item.Importe.ToString("N0")).PadLeft(13);
            var ganancia        = ("C$ " + (item.PrecioPublico * 80).ToString("N0")).PadLeft(13);            
            if (showReferencia && !string.IsNullOrWhiteSpace(item.Referencia))
            {
                var partes = item.Referencia.Replace(" ", "y").Split('y');
                foreach (var parte in partes)
                {
                    printer.Append($"#{parte.Trim().PadLeft(3)} {precio} {ganancia}");
                }
            }
        }
        //printer.Separator('-', 40);
        printer.NewLine();
        const int labelWidth = 12;
        const int valueWidth = 12;
        printer.Append($"{"TOTAL: ".PadRight(labelWidth)}{  ("C$ " + ((dtoInvoice.TransactionMaster.SubAmount - dtoInvoice.TransactionMaster.Discount).ToString("N0"))).PadLeft(valueWidth)}");
        printer.NewLine();
        printer.AlignCenter();

        if (printerQR.Trim().Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            var qrContent = printerQRUrl + "/inm/" + dtoInvoice.Codigo + "/unm/" + userNickname;
            printer.QrCode(qrContent, QrCodeSize.Size1);
            printer.NewLine();            
        }

        printer.Append(companyAddress);
        printer.Append(companyTelefono);
        //printer.NewLine();
        //printer.NewLine();

        printer.FullPaperCut();
        printer.Print();
        await Task.CompletedTask;
    }
}
