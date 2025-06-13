using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Repository;

public class RepositoryTbCustomer(DataBase dataBase, IRepositoryTbParameterSystem repositoryParameters) : RepositoryFacade<Api_AppMobileApi_GetDataDownloadCustomerResponse>(dataBase), IRepositoryTbCustomer
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
        search          = search.ToLower();
        var typeInvoice = (int)TypeTransaction.TransactionInvoiceBilling;
        var query = $"""
                      SELECT 
                          tbc.CustomerId,
                          tbc.CompanyId,
                          tbc.BranchId,
                          tbc.EntityId,
                          tbc.CustomerNumber,
                          tbc.Identification,
                          tbc.FirstName,
                          tbc.LastName,
                          tbc.Balance,
                          tbc.CurrencyId,
                          tbc.CurrencyName,
                          tbc.CustomerCreditLineId,
                          tbc.Location,
                          tbc.Phone,
                          tbc.Me,
                          tbc.Modificado,
                          tbc.Secuencia,
                          tbc.Remaining,
                          CASE
                              WHEN EXISTS (
                                  SELECT 1
                                  FROM tb_transaction_master ttm
                                  WHERE ttm.EntityId = tbc.EntityId
                                  AND ttm.TransactionId = {typeInvoice}
                              ) THEN 1
                              ELSE 0
                          END AS Facturado
                      FROM tb_customers tbc
                      where tbc.Identification like '%{search}%' OR 
                            tbc.FirstName like '%{search}%' OR
                            tbc.LastName like '%{search}%' OR
                            tbc.CustomerNumber like '%{search}%'
                      ORDER BY tbc.Secuencia
                      limit {skip}, {take}
                      """;
        return _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownloadCustomerResponse>(query);


    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterByInvoice()
    {
        var typeShare = (int)TypeTransaction.TransactionShare;
        var query = $"""
                    select  
                        tbc.CustomerId,
                        tbc.CompanyId,
                        tbc.BranchId,
                        tbc.EntityId,
                        tbc.CustomerNumber,
                        tbc.Identification,
                        tbc.FirstName,
                        tbc.LastName,
                        SUM(tbc.Balance) as Balance,
                        tbc.CurrencyId,
                        tbc.CurrencyName,
                        tbc.CustomerCreditLineId,
                        tbc.Location,
                        tbc.Phone,
                        tbc.Me,
                        tbc.Modificado,
                        tbc.Secuencia,
                        tbc.SecuenciaAbono,
                        SUM(tdc.Balance) as Remaining,
                        (
                            SELECT MIN(tdc.DateApply)
                            FROM document_credit_amortization tdc
                            WHERE tdc.CustomerNumber = tbc.CustomerNumber
                              AND tdc.Remaining > 0
                        ) AS FirstBalanceDate,
                        CASE
                            WHEN EXISTS (
                                SELECT 1
                                FROM tb_transaction_master ttm
                                WHERE ttm.EntityId = tbc.EntityId
                                AND ttm.TransactionId = {typeShare}
                            ) THEN 1
                            ELSE 0
                        END AS HasAbono
                    from tb_customers tbc join tb_document_credit tdc on tbc.EntityId = tdc.EntityId
                    GROUP BY 
                        tbc.CustomerId,
                        tbc.CompanyId,
                        tbc.BranchId,
                        tbc.EntityId,
                        tbc.CustomerNumber,
                        tbc.Identification,
                        tbc.FirstName,
                        tbc.LastName,
                        tbc.Balance,
                        tbc.CurrencyId,
                        tbc.CurrencyName,
                        tbc.CustomerCreditLineId,
                        tbc.Location,
                        tbc.Phone,
                        tbc.Me,
                        tbc.Modificado,
                        tbc.Secuencia,
                        tbc.SecuenciaAbono
                    order by FirstBalanceDate
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
        var typeShare = (int)TypeTransaction.TransactionShare;
        var query = $"""
                     select  
                         tbc.CustomerId,
                         tbc.CompanyId,
                         tbc.BranchId,
                         tbc.EntityId,
                         tbc.CustomerNumber,
                         tbc.Identification,
                         tbc.FirstName,
                         tbc.LastName,
                         SUM(tbc.Balance) as Balance,
                         tbc.CurrencyId,
                         tbc.CurrencyName,
                         tbc.CustomerCreditLineId,
                         tbc.Location,
                         tbc.Phone,
                         tbc.Me,
                         tbc.Modificado,
                         tbc.Secuencia,
                         tbc.SecuenciaAbono,
                         SUM(tdc.Balance) as Remaining,
                         (
                             SELECT MIN(tdc.DateApply)
                             FROM document_credit_amortization tdc
                             WHERE tdc.CustomerNumber = tbc.CustomerNumber
                               AND tdc.Remaining > 0
                         ) AS FirstBalanceDate,
                         CASE
                             WHEN EXISTS (
                                 SELECT 1
                                 FROM tb_transaction_master ttm
                                 WHERE ttm.EntityId = tbc.EntityId
                                 AND ttm.TransactionId = {typeShare}
                             ) THEN 1
                             ELSE 0
                         END AS HasAbono
                     from 
                        tb_customers tbc join tb_document_credit tdc on tbc.EntityId = tdc.EntityId
                     where 
                        tbc.CustomerNumber like '%{search}%' or tbc.FirstName like '%{search}%' or tbc.identification like '%{search}%'
                     GROUP BY 
                         tbc.CustomerId,
                         tbc.CompanyId,
                         tbc.BranchId,
                         tbc.EntityId,
                         tbc.CustomerNumber,
                         tbc.Identification,
                         tbc.FirstName,
                         tbc.LastName,
                         tbc.Balance,
                         tbc.CurrencyId,
                         tbc.CurrencyName,
                         tbc.CustomerCreditLineId,
                         tbc.Location,
                         tbc.Phone,
                         tbc.Me,
                         tbc.Modificado,
                         tbc.Secuencia,
                         tbc.SecuenciaAbono                      
                     order by FirstBalanceDate
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
        var typeInvoice = (int)TypeTransaction.TransactionInvoiceBilling;
        var query = $"""
                    SELECT 
                        tbc.CustomerId,
                        tbc.CompanyId,
                        tbc.BranchId,
                        tbc.EntityId,
                        tbc.CustomerNumber,
                        tbc.Identification,
                        tbc.FirstName,
                        tbc.LastName,
                        tbc.Balance,
                        tbc.CurrencyId,
                        tbc.CurrencyName,
                        tbc.CustomerCreditLineId,
                        tbc.Location,
                        tbc.Phone,
                        tbc.Me,
                        tbc.Modificado,
                        tbc.Secuencia,
                        tbc.Remaining,
                        CASE
                            WHEN EXISTS (
                                SELECT 1
                                FROM tb_transaction_master ttm
                                WHERE ttm.EntityId = tbc.EntityId
                                  AND ttm.TransactionId = {typeInvoice}
                            ) THEN 1
                            ELSE 0
                        END AS Facturado
                    FROM tb_customers tbc
                    ORDER BY tbc.Secuencia 
                    limit {skip}, {take}
                    """;
        return _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownloadCustomerResponse>(query);
    }

    public async Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterByShare()
    {
        var typeShare = (int)TypeTransaction.TransactionShare;
        var query = $"""
                    select  
                        tbc.CustomerId,
                        tbc.CompanyId,
                        tbc.BranchId,
                        tbc.EntityId,
                        tbc.CustomerNumber,
                        tbc.Identification,
                        tbc.FirstName,
                        tbc.LastName,
                        SUM(tbc.Balance) as Balance,
                        tbc.CurrencyId,
                        tbc.CurrencyName,
                        tbc.CustomerCreditLineId,
                        tbc.Location,
                        tbc.Phone,
                        tbc.Me,
                        tbc.Modificado,
                        tbc.Secuencia,
                        tbc.SecuenciaAbono,
                        tbc.OrdenAbono as OrdenAbono,
                        tbc.IsHaveShareNow as IsHaveShareNow,
                        tbc.FrecuencyNameIntoShare,
                        tbc.ShowFrecuenciInCustomerIntoShare,
                        SUM(tdc.Balance) as Remaining,
                        (
                            SELECT MIN(tdc.DateApply)
                            FROM document_credit_amortization tdc
                            WHERE tdc.CustomerNumber = tbc.CustomerNumber
                              AND tdc.Remaining > 0
                        ) AS FirstBalanceDate,
                        CASE
                            when tbc.IsHaveShareNow = 1 THEN 1
                            else
                                CASE
                                    WHEN EXISTS (
                                        SELECT 1
                                        FROM tb_transaction_master ttm
                                        WHERE ttm.EntityId = tbc.EntityId
                                        AND ttm.TransactionId = {typeShare}
                                    ) THEN 1
                                    ELSE 0
                                END 
                        END AS HasAbono
                    from 
                        tb_customers 
                        tbc join tb_document_credit tdc on tbc.EntityId = tdc.EntityId
                    GROUP BY 
                        tbc.CustomerId,
                        tbc.CompanyId,
                        tbc.BranchId,
                        tbc.EntityId,
                        tbc.CustomerNumber,
                        tbc.Identification,
                        tbc.FirstName,
                        tbc.LastName,
                        tbc.Balance,
                        tbc.CurrencyId,
                        tbc.CurrencyName,
                        tbc.CustomerCreditLineId,
                        tbc.Location,
                        tbc.Phone,
                        tbc.Me,
                        tbc.Modificado,
                        tbc.Secuencia,
                        tbc.SecuenciaAbono,
                        tbc.OrdenAbono,
                        tbc.IsHaveShareNow,
                        tbc.FrecuencyNameIntoShare,
                        tbc.ShowFrecuenciInCustomerIntoShare 
                    order by 
                        tbc.OrdenAbono,
                        FirstBalanceDate 

                    """;
        return await _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownloadCustomerResponse>(query);
    }
    
    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeFilterByCustomerShare(string search)
    {
        var typeShare = (int)TypeTransaction.TransactionShare;
        var query = @"
                     select  
                         tbc.CustomerId,
                         tbc.CompanyId,
                         tbc.BranchId,
                         tbc.EntityId,
                         tbc.CustomerNumber,
                         tbc.Identification,
                         tbc.FirstName,
                         tbc.LastName,
                         SUM(tbc.Balance) as Balance,
                         tbc.CurrencyId,
                         tbc.CurrencyName,
                         tbc.CustomerCreditLineId,
                         tbc.Location,
                         tbc.Phone,
                         tbc.Me,
                         tbc.Modificado,
                         tbc.Secuencia,
                         tbc.SecuenciaAbono,
                         tbc.OrdenAbono as OrdenAbono,
                         tbc.IsHaveShareNow as IsHaveShareNow,
                         tbc.FrecuencyNameIntoShare,
                         tbc.ShowFrecuenciInCustomerIntoShare,
                         SUM(tdc.Balance) as Remaining,
                         (
                             SELECT MIN(tdc.DateApply)
                             FROM document_credit_amortization tdc
                             WHERE tdc.CustomerNumber = tbc.CustomerNumber
                               AND tdc.Remaining > 0
                         ) AS FirstBalanceDate,
                          CASE
                            when tbc.IsHaveShareNow = 1 THEN 1
                            else
                                CASE
                                     WHEN EXISTS (
                                         SELECT 1
                                         FROM tb_transaction_master ttm
                                         WHERE ttm.EntityId = tbc.EntityId
                                         AND ttm.TransactionId = " + typeShare+ @"
                                     ) THEN 1
                                     ELSE 0
                                 END
                         END AS HasAbono
                     FROM 
                         tb_customers tbc join tb_document_credit tdc on tbc.EntityId = tdc.EntityId
                     WHERE  
                         tbc.CustomerNumber like '%"+search+ @"%' or tbc.FirstName like '%"+search+ @"%' or tbc.identification like '%"+search+ @"%' or tbc.LastName like '%"+search+ @"%' 
                     GROUP BY 
                         tbc.CustomerId,
                         tbc.CompanyId,
                         tbc.BranchId,
                         tbc.EntityId,
                         tbc.CustomerNumber,
                         tbc.Identification,
                         tbc.FirstName,
                         tbc.LastName,
                         tbc.Balance,
                         tbc.CurrencyId,
                         tbc.CurrencyName,
                         tbc.CustomerCreditLineId,
                         tbc.Location,
                         tbc.Phone,
                         tbc.Me,
                         tbc.Modificado,
                         tbc.Secuencia,
                         tbc.SecuenciaAbono,
                         tbc.OrdenAbono,
                         tbc.IsHaveShareNow,
                         tbc.FrecuencyNameIntoShare,
                         tbc.ShowFrecuenciInCustomerIntoShare
                     ORDER BY 
                         tbc.OrdenAbono,
                         FirstBalanceDate 
                     ";
        return _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownloadCustomerResponse>(query);
    }
    
    public Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> PosMeTakeModificados()
    {
        return _dataBase.Database.Table<Api_AppMobileApi_GetDataDownloadCustomerResponse>()
            .Where(response => response.Modificado).ToListAsync();
    }
}