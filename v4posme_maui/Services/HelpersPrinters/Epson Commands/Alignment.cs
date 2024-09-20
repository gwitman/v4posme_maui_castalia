using v4posme_maui.Services.HelpersPrinters.Extensions;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;

namespace v4posme_maui.Services.HelpersPrinters.Epson_Commands
{
    public class Alignment : IAlignment

    {
        public byte[] Left()
        {
            return new byte[] { 27, 'a'.ToByte(), 0 };
        }

        public byte[] Right()
        {
            return new byte[] { 27, 'a'.ToByte(), 2 };
        }

        public byte[] Center()
        {
            return new byte[] { 27, 'a'.ToByte(), 1 };
        }
        public byte[] Avanza(int puntos)
        {
            return new byte[] { 27, 74, (byte)puntos };//8puntos = 1mm
        }
    }
}