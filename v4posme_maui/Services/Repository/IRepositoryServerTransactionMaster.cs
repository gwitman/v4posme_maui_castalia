using v4posme_maui.Models;



namespace v4posme_maui.Services.Repository
{
    
    public interface IRepositoryServerTransactionMaster : IRepositoryFacade<Api_AppMobileApi_GetDataDownloadServerTransactionMasterResponse>
    {
        Task<List<Api_AppMobileApi_GetDataDownloadServerTransactionMasterResponse>> PosMeFilterByCurrencyIDAndTransactionID(int currencyID,int transactionID);
    }
}
