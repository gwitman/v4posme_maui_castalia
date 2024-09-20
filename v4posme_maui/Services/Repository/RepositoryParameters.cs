using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryParameters(DataBase dataBase) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadParametersResponse>(dataBase), IRepositoryParameters
{
    private readonly DataBase _dataBase = dataBase;

    public Task<Api_AppMobileApi_GetDataDownloadParametersResponse?> PosMeFindByKey(string key)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadParametersResponse>()
            .Where(response => response.Name == key)
            .FirstOrDefaultAsync();
    }
}