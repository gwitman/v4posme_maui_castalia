using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryItems(DataBase dataBase)
    : RepositoryFacade<Api_AppMobileApi_GetDataDownloadItemsResponse>(dataBase), IRepositoryItems
{
    private readonly DataBase _dataBase = dataBase;

    public Task<int> PosMeExistBarCode(string barcode, int itemId = 0)
    {
        var query = _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Where(response => response.BarCode == barcode);
        if (itemId > 0)
        {
            query = query.Where(response => response.ItemId != itemId);
        }
        return query.CountAsync();
    }

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
    public async Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeFilterdByItemNumberAndBarCodeAndNameByTop(string? textSearch,int size /*numero o elemento inicial */,int top /*elementos por paginas*/)
    {
        textSearch = textSearch!.ToLower();
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Where(response => response.ItemNumber!.ToLower().Contains(textSearch)
                               || response.BarCode.ToLower().Contains(textSearch)
                               || response.Name.ToLower().Contains(textSearch))
            .OrderBy(response => response.Name)
            .Skip(size)
            .Take(top)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeDescendingBySizeAndTop(int size,int take)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Skip(size)
            .Take(take)
            .OrderByDescending(response => response.ItemNumber)
            .ToListAsync();
    }
    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeAscBySizeAndTop(int size, int take)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Skip(size)
            .Take(take)
            .OrderBy(response => response.Name)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeTakeModificado()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .Where(response => response.Modificado).ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeDescending10(int take = 10)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .OrderByDescending(response => response.ItemNumber)
            .Take(take)            
            .ToListAsync();
    }
    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeNameAsc10(int take = 10)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .OrderBy(response => response.Name)
            .Take(take)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadItemsResponse>> PosMeQuantityDistintoZero()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadItemsResponse>()
            .OrderByDescending(response => response.ItemNumber)
            .Where(respose => respose.Quantity != 0m)
            .ToListAsync();
    }
}