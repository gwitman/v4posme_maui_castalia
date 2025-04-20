using v4posme_maui.Models;
using Unity;

namespace v4posme_maui.Services.SystemNames;

public static class VariablesGlobales
{
    public static string? CompanyKey;
    public static Api_CoreAccount_LoginMobileObjUserResponse? User;
    public static readonly UnityContainer UnityContainer;
    public static string? LogoTemp;
    public static ViewTempDtoAbono? DtoAplicarAbono;
    public static ViewTempDtoInvoice DtoInvoice;
    public static TbCompany? TbCompany=new();
    public static decimal TipoCambio = new(36.5);
    static VariablesGlobales()
    {
        UnityContainer = new UnityContainer();
        DtoInvoice = new();
        Item = new();
        CustomerOrderShares = new();
    }


    public static bool EnableBackButton { get; set; }
    public static Api_AppMobileApi_GetDataDownloadItemsResponse Item { get; set; }
    public static List<CustomerOrderShare> CustomerOrderShares { get; set; }
}