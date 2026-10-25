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

    // Devuelve todas las facturas con saldo del cliente, sin el limite de 10 de
    // PosMeFindByEntityId. Se usa solo en la pantalla de Abonos 2/5 (seleccion de
    // factura), donde el usuario necesita ver todas las facturas para abonar.
    public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>> PosMeFindAllByEntityId(int entityId)
    {
        return await dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>()
            .Where(response => response.EntityId == entityId && response.Balance > decimal.Zero)
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

	public Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>> PosMeFindByDate(DateTime startDate, DateTime endDate)
	{
		return dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>()
			.Where(response => response.DateApply >= startDate && response.DateApply <= endDate)
			.ToListAsync();
	}
}