using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryItems(DataBase dataBase)
    : RepositoryFacade<Api_AppMobileApi_GetDataDownloadItemsResponse>(dataBase), IRepositoryItems
{
    private readonly DataBase _dataBase = dataBase;

    public async Task<Api_AppMobileApi_GetDataDownloadItemsResponse?> PosMeFindByBarCode(string barCode)
    {
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .FirstOrDefaultAsync(response => response.BarCode == barCode);
    }

    public  Task<Api_AppMobileApi_GetDataDownloadItemsResponse> PosMeFindByItemNumber(string itemNumber)
    {
        return  _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .FirstOrDefaultAsync(response => response.ItemNumber == itemNumber);
    }

    public Task<Api_AppMobileApi_GetDataDownloadItemsResponse> PosMeFindByItemId(int itemId)
    {
        return  _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .FirstOrDefaultAsync(response => response.ItemId == itemId);
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumber(string? textSearch)
    {
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Where(response => response.ItemNumber!.Contains(textSearch!))
            .ToListAsync();
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumberAndBarCodeAndName(string? textSearch)
    {
        textSearch = textSearch!.ToLower();
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Where(response => response.ItemNumber!.ToLower().Contains(textSearch)
                               || response.BarCode.ToLower().Contains(textSearch)
                               || response.Name.ToLower().Contains(textSearch))
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeDescending10(int top = 10)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Take(top)
            .OrderByDescending(response => response.ItemNumber)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeTakeModificado()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Where(response => response.Modificado).ToListAsync();
    }
}