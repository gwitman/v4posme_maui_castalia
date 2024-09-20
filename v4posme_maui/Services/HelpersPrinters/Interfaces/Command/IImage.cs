using SkiaSharp;

namespace v4posme_maui.Services.HelpersPrinters.Interfaces.Command
{
    internal interface IImage
    {
        byte[] Print(SKBitmap image);
    }
}
