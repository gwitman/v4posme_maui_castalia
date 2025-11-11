using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbTransactionMasterDetail : IRepositoryFacade<TbTransactionMasterDetail>
{
    Task<List<TbTransactionMasterDetail>> PosMeItemByTransactionId(int transactionId);
    Task<List<TbTransactionMasterDetail>> PosMeByTransactionIDAndItemID(int transactionId, int itemID);
    Task<List<TbTransactionMasterDetail>> PosMeByTransactionIDAndItemID_BetweenDate(int transactionId, int itemID, DateTime startOn, DateTime endOn);
}