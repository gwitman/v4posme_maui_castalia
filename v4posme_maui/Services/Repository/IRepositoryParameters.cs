using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryParameters : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadParametersResponse>
{
    Task<Api_AppMobileApi_GetDataDownloadParametersResponse?> PosMeFindByKey(string key);
}