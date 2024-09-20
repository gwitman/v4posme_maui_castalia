namespace v4posme_maui.Services.HelpersPrinters.Interfaces.Command
{
    internal interface IAlignment
    {
        byte[] Left();
        byte[] Right();
        byte[] Center();
        byte[] Avanza(int puntos);
    }
}

