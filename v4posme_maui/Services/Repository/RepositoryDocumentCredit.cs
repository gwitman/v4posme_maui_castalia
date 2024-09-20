using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryDocumentCredit(DataBase dataBase) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>(dataBase), IRepositoryDocumentCredit
{
    public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>> PosMeFindByEntityId(int entityId)
    {
        return await dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>()
            .Where(response => response.EntityId == entityId && response.Balance > decimal.Zero)
            .Take(10)
            .ToListAsync();
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>> PosMeFilterDocumentNumber(string filter)
    {
        return await dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>()
            .Where(response => response.DocumentNumber!.Contains(filter))
            .ToListAsync();
    }

    public async Task<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> PosMeFindDocumentNumber(string filter)
    {
        return await dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>()
            .Where(response => response.DocumentNumber == filter)
            .FirstOrDefaultAsync();
    }

    public Task<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> PosMeFindByAmortizationId(int id)
    {
        return dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>()
            .Where(response => response.CreditAmortizationId == id)
            .FirstOrDefaultAsync();
    }
}