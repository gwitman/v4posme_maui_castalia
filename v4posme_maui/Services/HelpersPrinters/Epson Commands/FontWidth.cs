using v4posme_maui.Services.HelpersPrinters.Extensions;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;

namespace v4posme_maui.Services.HelpersPrinters.Epson_Commands
{
    public class FontWidth : IFontWidth
    {
        public byte[] Normal()
        {
            return new byte[] { 27, '!'.ToByte(), 0 };
        }

        public byte[] DoubleWidth2()
        {
            return new byte[] { 29, '!'.ToByte(), 16 };
        }

        public byte[] DoubleWidth3()
        {
            return new byte[] { 29, '!'.ToByte(), 32 };
        }
    }
}

