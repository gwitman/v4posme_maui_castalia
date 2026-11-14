using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryTbZone(DataBase dataBase) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadZoneResponse>(dataBase), IRepositoryTbZone
{
    private readonly DataBase _dataBase = dataBase;

    // Zonas en el mismo orden que la web (sequence y luego nombre)
    public Task<List<Api_AppMobileApi_GetDataDownloadZoneResponse>> PosMeFindAllOrdered()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadZoneResponse>()
            .OrderBy(zone => zone.Sequence)
            .ThenBy(zone => zone.Display)
            .ToListAsync();
    }
}
