using v4posme_maui.Services.HelpersPrinters.Extensions;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;

namespace v4posme_maui.Services.HelpersPrinters.Epson_Commands
{
    public class PaperCut : IPaperCut
    {
        public byte[] Full()
        {
            return new byte[] { 29, 'V'.ToByte(), 65, 0 };
        }

        public byte[] Partial()
        {
            return new byte[] { 29, 'V'.ToByte(), 65, 1 };
        }
    }
}

