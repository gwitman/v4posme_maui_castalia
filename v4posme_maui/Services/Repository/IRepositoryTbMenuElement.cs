using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbMenuElement : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadMenuElementResponse>
{   
    Task<Api_AppMobileApi_GetDataDownloadMenuElementResponse> PosMeFindById(int menuElementID);
    
}
