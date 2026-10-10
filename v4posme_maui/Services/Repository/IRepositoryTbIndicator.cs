using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbIndicator : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadIndicatorResponse>
{
    Task<Api_AppMobileApi_GetDataDownloadIndicatorResponse> PosMeFindBySystemName(string systemName);
}
