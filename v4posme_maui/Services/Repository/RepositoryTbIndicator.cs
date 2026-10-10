using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryTbIndicator(DataBase dataBase) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadIndicatorResponse>(dataBase), IRepositoryTbIndicator
{
    private readonly DataBase _dataBase = dataBase;

    public Task<Api_AppMobileApi_GetDataDownloadIndicatorResponse> PosMeFindBySystemName(string systemName)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadIndicatorResponse>()
            .FirstOrDefaultAsync(indicator => indicator.SystemName == systemName);
    }
}
