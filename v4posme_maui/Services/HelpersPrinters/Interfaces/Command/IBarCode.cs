using v4posme_maui.Services.HelpersPrinters.Enums;

namespace v4posme_maui.Services.HelpersPrinters.Interfaces.Command
{
    interface IBarCode
    {
        byte[] Code128V2(byte[] code, Positions printString);
        byte[] Code128(string code,Positions printString);
        byte[] Code39(string code, Positions printString);
        byte[] Code39CustomPosMe2px1p(string code, Positions printString);
        byte[] Ean13(string code, Positions printString);
    }
}

