using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryDocumentCreditAmortization : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>
{
    Task<int> PosMeCountByDocumentNumber(string document);
    Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFilterByCustomerNumber(string filter);
    Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFilterByDocumentNumber(string document);
    Task<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> PosMeFindByDocumentNumber(string document);
    Task<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> PosMeFindByAmortizationId(int id);
	Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFindByDate(DateTime startDate, DateTime endDate);
}