using v4posme_maui.Models;
namespace v4posme_maui.Services.Repository;



internal interface IRepositoryTbCatalogItem : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadCatalogItemResponse>
{
    Task<List<Api_AppMobileApi_GetDataDownloadCatalogItemResponse>> PosMeFindByName(string catalogName);
}

