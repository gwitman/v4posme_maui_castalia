using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;

namespace v4posme_maui.Services.HelpersPrinters.Epson_Commands
{
    public class Drawer : IDrawer
    {
        public byte[] Open()
        {
            return new byte[] { 27, 112, 0, 60, 120 };
        }
    }
}

