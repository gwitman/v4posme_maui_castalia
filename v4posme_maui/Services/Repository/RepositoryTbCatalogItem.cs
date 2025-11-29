using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
namespace v4posme_maui.Services.Repository;
public class RepositoryTbCatalogItem(DataBase dataBase, IRepositoryTbParameterSystem repositoryParameters) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadCatalogItemResponse>(dataBase), IRepositoryTbCatalogItem
{
    private readonly DataBase _dataBase = dataBase;


    public Task<List<Api_AppMobileApi_GetDataDownloadCatalogItemResponse>> PosMeFindByName(string catalogName)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCatalogItemResponse>()
            .Where(customers => customers.catalogName == catalogName).ToListAsync();
    }

}
