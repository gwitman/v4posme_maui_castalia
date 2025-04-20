using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryItems : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadItemsResponse>
{
    Task<int> PosMeExistBarCode(string barcode, int itemId = 0);
    
    Task<Api_AppMobileApi_GetDataDownloadItemsResponse?> PosMeFindByBarCode(string barCode);
    
    Task<Api_AppMobileApi_GetDataDownloadItemsResponse> PosMeFindByItemNumber(string itemNumber);
    
    Task<Api_AppMobileApi_GetDataDownloadItemsResponse> PosMeFindByItemId(int itemId);
    
    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumber(string? textSearch);
    
    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumberAndBarCodeAndName(string? textSearch);
    
    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumberAndBarCodeAndNameByTop(string? textSearch,int size,int top);

    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeDescendingBySizeAndTop(int size,int top );

    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeTakeModificado();
    
    Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeDescending10(int take = 10);
}