using v4posme_maui.Services.HelpersPrinters.Enums;
using v4posme_maui.Services.HelpersPrinters.Extensions;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;
using IImage = v4posme_maui.Services.HelpersPrinters.Interfaces.Command.IImage;

namespace v4posme_maui.Services.HelpersPrinters.Epson_Commands
{
    internal class EscPos : IPrintCommand
    {
        public IFontMode FontMode { get; set; }
        public IFontWidth FontWidth { get; set; }
        public Alignment Alignment { get; set; }
        public IPaperCut PaperCut { get; set; }
        public IDrawer Drawer { get; set; }
        public IQrCode QrCode { get; set; }
        public IBarCode BarCode { get; set; }
        public IInitializePrint InitializePrint { get; set; }
        public IImage Image { get; set; }
        public ILineHeight  LineHeight { get; set; }
        public int ColsNomal => 48;
        public int ColsCondensed => 64;
        public int ColsExpanded => 24;        

        public EscPos()
        {
            FontMode = new FontMode();
            FontWidth = new FontWidth();
            Alignment = new Alignment();
            PaperCut = new PaperCut();
            Drawer = new Drawer();
            QrCode = new QrCode();
            BarCode = new BarCode();
            Image = new v4posme_maui.Services.HelpersPrinters.Epson_Commands.Image();
            LineHeight = new LineHeight();
            InitializePrint = new InitializePrint();
        }

        public byte[] Separator(char speratorChar= '-')
        {
            return FontMode.Condensed(PrinterModeState.On)
                .AddBytes(new string(speratorChar, ColsCondensed))
                .AddBytes(FontMode.Condensed(PrinterModeState.Off))
                .AddCrLF();
        }

        public byte[] AutoTest()
        {
            return new byte[] { 29, 40, 65, 2, 0, 0, 2 };
        }

    }
}

