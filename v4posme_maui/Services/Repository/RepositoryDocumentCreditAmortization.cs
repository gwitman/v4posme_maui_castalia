using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryDocumentCreditAmortization(DataBase dataBase) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>(dataBase), IRepositoryDocumentCreditAmortization
{
    private readonly DataBase _dataBase = dataBase;

    public async Task<int> PosMeCountByDocumentNumber(string document)
    {
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .CountAsync(response => response.DocumentNumber!.Contains(document));
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFilterByCustomerNumber(string filter)
    {
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .Where(response => response.CustomerNumber == filter && response.Remaining > decimal.Zero)
            .OrderBy(response => response.DateApply)
            .ToListAsync();
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFilterByDocumentNumber(string document)
    {
        var query = """
                    select tdc.CurrencyName,
                           dca.CreditAmortizationID,
                           dca.CustomerNumber,
                           dca.firstname,
                           dca.lastname,
                           dca.birthdate,
                           dca.DocumentNumber,
                           dca.CurrencyId,
                           dca.ReportSinRiesgo,
                           dca.DateApply,
                           dca.Remaining
                    from document_credit_amortization dca
                             join tb_document_credit tdc on dca.DocumentNumber = tdc.DocumentNumber
                    where dca.DocumentNumber = ? and dca.Remaining>0
                    """;
        return await _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>(query, document);
    }

    public async Task<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> PosMeFindByDocumentNumber(string document)
    {
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .FirstOrDefaultAsync(response => response.DocumentNumber == document);
    }

    public Task<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> PosMeFindByAmortizationId(int id)
    {
        return  _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .FirstOrDefaultAsync(response => response.CreditAmortizationId == id);
    }
}