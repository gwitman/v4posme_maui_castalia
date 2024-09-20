using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbTransactionMasterDetail : IRepositoryFacade<TbTransactionMasterDetail>
{
    Task<List<TbTransactionMasterDetail>> PosMeItemByTransactionId(int transactionId);
}