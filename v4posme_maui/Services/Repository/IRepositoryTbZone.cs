using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbZone : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadZoneResponse>
{
    Task<List<Api_AppMobileApi_GetDataDownloadZoneResponse>> PosMeFindAllOrdered();
}
