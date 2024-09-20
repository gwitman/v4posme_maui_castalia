namespace v4posme_maui.Services.HelpersPrinters.Interfaces.Command
{
    internal interface IPaperCut
    {
        byte[] Full();
        byte[] Partial();
    }
}

