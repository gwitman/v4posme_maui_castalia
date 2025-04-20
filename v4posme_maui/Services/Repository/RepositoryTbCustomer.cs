using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryTbCustomer(DataBase dataBase) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadCustomerResponse>(dataBase), IRepositoryTbCustomer
{
    private readonly DataBase _dataBase = dataBase;

    public Task<Api_AppMobileApi_GetDataDownloadCustomerResponse> PosMeFindCustomer(string customerNumber)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .Where(customers => customers.CustomerNumber == customerNumber)
            .FirstOrDefaultAsync();
    }

    public Task<int> PosMeExisteCustomerIdentification(string identification, int customerId = 0)
    {
        var query = _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>();
        if (customerId > 0)
        {
            query = query.Where(c => c.CustomerId != customerId);
        }

        return query.Where(response => response.Identification==identification).CountAsync();
    }

    public Task<Api_AppMobileApi_GetDataDownloadCustomerResponse> PosMeFindEntityId(int entityID)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .Where(customers => customers.EntityId == entityID)
            .FirstOrDefaultAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterBySearch(string search, int skip, int take)
    {
        search = search.ToLower();
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .Where(response => response.CustomerNumber!.ToLower().Contains(search)
                               || response.Identification!.ToLower().Contains(search)
                               || response.FirstName!.ToLower().Contains(search)
                               || response.LastName!.ToLower().Contains(search))
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterByInvoice()
    {
        var query = """
                    select distinct 
                        tbc.CustomerId, 
                        tbc.CompanyId, 
                        BranchId, 
                        tbc.EntityId, 
                        CustomerNumber, 
                        Identification, 
                        tbc.CustomerCreditLineId, 
                        FirstName, 
                        LastName,
                        tbc.CurrencyName, 
                        tbc.CurrencyId, 
                        tdc.Balance, 
                        Modificado,
                        tdc.Remaining
                    from tb_customers tbc join tb_document_credit tdc on tbc.EntityId = tdc.EntityId
                    """;
        return await _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownloadCustomerResponse>(query);
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilter10()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .Take(10)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterByCustomerInvoice(string search)
    {
        var query = $"""
                     select tbc.CustomerId, tbc.CompanyId, BranchId, tbc.EntityId, CustomerNumber, Identification, 
                                     FirstName, LastName,tbc.CurrencyName, tbc.CurrencyId, tbc.Balance, Modificado 
                     from tb_customers tbc
                     where tbc.CustomerNumber like '%{search}%' or tbc.FirstName like '%{search}%' or tbc.identification like '%{search}%'
                     """;
        return _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownloadCustomerResponse>(query);
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeDescTake10()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .OrderByDescending(response => response.CustomerNumber)
            .Take(10)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeAscTake10(int top = 10)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .OrderBy(response => response.CustomerNumber)
            .Take(top)
            .ToListAsync();
    }
    
    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeCustomerAscLoad(int skip, int take)
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .OrderBy(response => response.CustomerNumber)
            .Skip(skip)
            .Take(take)
            .ToListAsync();
    }

    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeTakeModificados()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .Where(response => response.Modificado).ToListAsync();
    }
}