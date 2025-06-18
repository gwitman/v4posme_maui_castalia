using v4posme_maui.Models;



namespace v4posme_maui.Services.Repository
{
    
    public interface IRepositoryServerTransactionMaster : IRepositoryFacade<Api_AppMobileApi_GetDataDownload_ServerTransactionMaster_Response>
    {
        Task<List<Api_AppMobileApi_GetDataDownload_ServerTransactionMaster_Response>> PosMeFilterByCurrencyIDAndTransactionID(int currencyID,int transactionID);
    }
}
