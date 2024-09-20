using v4posme_maui.Services.HelpersPrinters.Enums;

namespace v4posme_maui.Services.HelpersPrinters.Interfaces.Command
{
    internal interface IQrCode
    {
        byte[] Print(string qrData);
        byte[] Print(string qrData, QrCodeSize qrCodeSize);
    }
}

