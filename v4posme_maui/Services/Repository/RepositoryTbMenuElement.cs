using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
namespace v4posme_maui.Services.Repository;
public class RepositoryTbMenuElement(DataBase dataBase, IRepositoryTbParameterSystem repositoryParameters) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadMenuElementResponse>(dataBase), IRepositoryTbMenuElement
{
    private readonly DataBase _dataBase = dataBase;


    public Task<Api_AppMobileApi_GetDataDownloadMenuElementResponse> PosMeFindById(int menuElementID)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadMenuElementResponse>()
            .Where(customers => customers.MenuElementId == menuElementID)
            .FirstOrDefaultAsync();
    }

}
