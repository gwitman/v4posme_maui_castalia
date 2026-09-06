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
    public static TbCompany? TbCompany  = new();
    public static decimal TipoCambio    = new(36.5);

    static VariablesGlobales()
    {
        UnityContainer      = new UnityContainer();
        DtoInvoice          = new();
        Item                = new();
        OrdenarAbonos       = true;
        OrdenarClientes     = true;
        CustomerOrderShares = new();
    }


    public static bool EnableBackButton { get; set; }
    public static Api_AppMobileApi_GetDataDownloadItemsResponse Item { get; set; }

    // Lista de productos actualmente cargada en la pantalla de listado (ItemsPage).
    // Se usa para navegar entre productos (anterior/siguiente) desde las pantallas
    // de detalle y edicion sin necesidad de volver al listado principal.
    public static List<Api_AppMobileApi_GetDataDownloadItemsResponse> ItemsNavegacion { get; set; } = new();

    // Indice del producto actualmente seleccionado dentro de ItemsNavegacion.
    // Se comparte entre las pantallas de detalle y edicion para que la navegacion
    // (anterior/siguiente) continue desde la misma posicion al cambiar de pantalla.
    public static int ItemsNavegacionIndex { get; set; }

    public static List<CustomerOrderShare> CustomerOrderShares { get; set; }
    public static bool OrdenarAbonos { get; set; }
    public static bool OrdenarClientes { get; set; }

    // Indica si el flujo de facturacion rapida (ir directo a seleccion de producto)
    // ya inicializo el DtoInvoice con los valores por defecto. Se usa para conservar
    // los productos seleccionados al volver desde el menu desplegable de la pantalla 4/6.
    public static bool InvoiceFlowInicializado { get; set; }

    // Indica que la lista de clientes (1/6) fue abierta desde el menu desplegable de la
    // pantalla de seleccion de producto (4/6) para cambiar el cliente. En ese caso la
    // pantalla debe mostrar la lista en lugar de saltar automaticamente a 4/6.
    public static bool InvoiceSeleccionandoCliente { get; set; }

    // Cliente generico por defecto para facturacion rapida.
    public const string ClienteGenericoNumberDefault = "CLI00000000";

}