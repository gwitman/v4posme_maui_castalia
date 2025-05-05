using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryDocumentCreditAmortization(DataBase dataBase) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>(dataBase), IRepositoryDocumentCreditAmortization
{
    private readonly DataBase _dataBase = dataBase;

    public Task<int> PosMeCountByDocumentNumber(string document)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .CountAsync(response => response.DocumentNumber!.Contains(document));
    }
    public Task<int> PosMeCountByDocumentNumberRemainingMayorAZero(string document)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .Where(dc=> dc.Remaining > 0m)
            .CountAsync(response => response.DocumentNumber!.Contains(document));
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFilterByCustomerNumber(string filter)
    {
        return await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .Where(response => response.CustomerNumber == filter && response.Remaining > decimal.Zero)
            .OrderBy(response => response.DateApply)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFilterByDocumentNumberAll(string document)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .Where(response => response.DocumentNumber == document)
            .ToListAsync();
    }
    public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFilterByDocumentNumber(string document)
    {
        var query = """
                    select tdc.CurrencyName,
                            dca.creditamortizationid, 
                            dca.customernumber, 
                            dca.firstname, 
                            dca.lastname, 
                            dca.birthdate, 
                            dca.documentnumber, 
                            dca.currencyid, 
                            dca.reportsinriesgo, 
                            dca.dateapply, 
                            dca.remaining, 
                            dca.balance, 
                            dca.sequence,
                            dca.montocuota
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

	public async Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFindByDate(DateTime startDate, DateTime endDate)
	{
		var results = await _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
		 .Where(response => response.DateApply >= startDate && response.DateApply <= endDate)
         .ToListAsync();

        return results;
	}

    public Task<List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>> PosMeFindByMaxDate(DateTime date)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>()
            .Where(response => response.DateApply <= date)
            .ToListAsync();
    }
} 