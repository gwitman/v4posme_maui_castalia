using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Repository
{
    
    public class RepositoryServerTransactionMaster(DataBase dataBase, IRepositoryTbParameterSystem repositoryParameters) : RepositoryFacade<Api_AppMobileApi_GetDataDownload_ServerTransactionMaster_Response>(dataBase), IRepositoryServerTransactionMaster
    {
        private readonly DataBase _dataBase = dataBase;
        public Task<List<Api_AppMobileApi_GetDataDownload_ServerTransactionMaster_Response>> PosMeFilterByCurrencyIDAndTransactionID(int currencyID, int transactionID)
        {
            
            
            var query = $"""
                      SELECT 
                          t.TransactionID,
                          t.TransactionMasterID,
                          t.CurrencyID,
                          t.Amount
                      FROM 
                            tb_server_transaction_master t
                      where 
                            t.TransactionID =  {transactionID} AND 
                            t.CurrencyID = {currencyID}
                      """;
            return _dataBase.Database.QueryAsync<Api_AppMobileApi_GetDataDownload_ServerTransactionMaster_Response>(query);


        }


    }
}
