
using v4posme_maui.Services.HelpersPrinters.Epson_Commands;

namespace v4posme_maui.Services.HelpersPrinters.Interfaces.Command
{
    internal interface IPrintCommand
    {
        int ColsNomal { get; }
        int ColsCondensed { get; }
        int ColsExpanded { get; }
        IFontMode FontMode { get; set; }
        IFontWidth FontWidth { get; set; }
        Alignment Alignment { get; set; }
        IPaperCut PaperCut { get; set; }
        IDrawer Drawer { get; set; }
        IQrCode QrCode { get; set; }
        IBarCode BarCode { get; set; }
        IImage Image { get; set; }
        ILineHeight LineHeight { get; set; }
        IInitializePrint InitializePrint { get; set; }
        byte[] Separator(char speratorChar = '-');
        byte[] AutoTest();
    }
}

