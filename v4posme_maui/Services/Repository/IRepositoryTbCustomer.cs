using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbCustomer : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadCustomerResponse>
{
    Task<Api_AppMobileApi_GetDataDownloadCustomerResponse> PosMeFindCustomer(string customerNumber);
    
    Task<int> PosMeExisteCustomerIdentification(string identification, int coustomerId=0);

    Task<Api_AppMobileApi_GetDataDownloadCustomerResponse> PosMeFindEntityId(int entityId);

    Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterBySearch(string search, int skip, int take);

    Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterByInvoice();
    
    Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterByCustomerInvoice(string search);

    Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeDescTake10();
    
    Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeAscTake10(int top = 10);

    Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeCustomerAscLoad(int skip, int take);
    
    Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeTakeModificados();
    
}