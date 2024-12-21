using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryDocumentCredit : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>
{
    Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>> PosMeFindByEntityId(int entityId);

    Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>> PosMeFilterDocumentNumber(string filter);
    Task<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> PosMeFindDocumentNumber(string filter);
    Task<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> PosMeFindByAmortizationId(int id);

	//Carlos Conto
	Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>> PosMeFindByDate(DateTime startDate, DateTime enDate);
}