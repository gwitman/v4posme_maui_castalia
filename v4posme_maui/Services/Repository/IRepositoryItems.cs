using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryItems : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadItemsResponse>
{
    Task<Api_AppMobileApi_GetDataDownloadItemsResponse?> PosMeFindByBarCode(string barCode);
    
    Task<Api_AppMobileApi_GetDataDownloadItemsResponse> PosMeFindByItemNumber(string itemNumber);
    
    Task<Api_AppMobileApi_GetDataDownloadItemsResponse> PosMeFindByItemId(int itemId);
    
    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumber(string? textSearch);
    
    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumberAndBarCodeAndName(string? textSearch);

    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeDescending10(int top = 10);

    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeTakeModificado();
}