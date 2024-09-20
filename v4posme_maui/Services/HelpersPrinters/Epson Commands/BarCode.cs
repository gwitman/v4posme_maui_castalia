using v4posme_maui.Services.HelpersPrinters.Enums;
using v4posme_maui.Services.HelpersPrinters.Extensions;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;

namespace v4posme_maui.Services.HelpersPrinters.Epson_Commands
{
    public class BarCode : IBarCode
    {
        public byte[] Code128(string code,Positions printString=Positions.NotPrint)
        {
            return new byte[] { 29, 119, 2 } // Width
                .AddBytes(new byte[] { 29, 104, 50 }) // Height
                .AddBytes(new byte[] { 29, 102, 1 }) // font hri character
                .AddBytes(new byte[] { 29, 72, printString.ToByte() }) // If print code informed
                .AddBytes(new byte[] { 29, 107, 73 }) // printCode
                .AddBytes(new[] { (byte)(code.Length + 2) })
                .AddBytes(new[] { '{'.ToByte(), 'C'.ToByte() })
                .AddBytes(code)
                .AddLF();
        }

        public byte[] Code39(string code, Positions printString = Positions.NotPrint)
        {
            return new byte[] { 29, 119, 2 } // Width
                .AddBytes(new byte[] { 29, 104, 50 }) // Height
                .AddBytes(new byte[] { 29, 102, 0 }) // font hri character
                .AddBytes(new byte[] { 29, 72, printString.ToByte() }) // If print code informed
                .AddBytes(new byte[] { 29, 107, 4 })
                .AddBytes(code)
                .AddBytes(new byte[] { 0 })
                .AddLF();
        }

        public byte[] Code39CustomPosMe2px1p(string code, Positions printString = Positions.NotPrint)
        {
            return new byte[] { 29, 119, 2 } // Width
                .AddBytes(new byte[] { 29, 104, 60 }) // Height
                .AddBytes(new byte[] { 29, 102, 0 }) // font hri character
                .AddBytes(new byte[] { 29, 72, printString.ToByte() }) // If print code informed
                .AddBytes(new byte[] { 29, 107, 4 })
                .AddBytes(code)
                .AddBytes(new byte[] { 0 })
                .AddLF();
        }


        public byte[] Ean13(string code, Positions printString = Positions.NotPrint)
        {
            if (code.Trim().Length != 13)
                return new byte[0];

            return new byte[] { 29, 119, 2 } // Width
                .AddBytes(new byte[] { 29, 104, 50 }) // Height
                .AddBytes(new byte[] { 29, 72, printString.ToByte() }) // If print code informed
                .AddBytes(new byte[] { 29, 107, 67, 12 })
                .AddBytes(code.Substring(0, 12))
                .AddLF();
        }
    }
}

