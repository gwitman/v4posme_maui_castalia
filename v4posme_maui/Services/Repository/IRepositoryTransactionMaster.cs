using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbTransactionMaster : IRepositoryFacade<TbTransactionMaster>
{
    Task<List<TbTransactionMaster>> PosMeFilterByCodigoAndNombreClienteFacturas(string filter);
    Task<List<TbTransactionMaster>> PosMeFilterByCodigoAndNombreClienteAbonos(string filter);
    Task<List<TbTransactionMaster>> PosMeFilterFacturas();
    Task<List<TbTransactionMaster>> PosMeFilterAbonos();
    Task<List<TbTransactionMaster>> PosMeFilterAbonosByCustomer(int entityId);
    Task<List<TbTransactionMaster>> PosMeFilterTop10Facturas();
    Task<List<TbTransactionMaster>> PosMeFilterTop10Abonos();
    Task<List<TbTransactionMaster>> PosMeFilterTop10ByCodigoAndNombreClienteFacturas(string filter);
    Task<List<TbTransactionMaster>> PosMeFilterTop10ByCodigoAndNombreClienteAbonos(string filter);
    Task<TbTransactionMaster> PosMeFindByTransactionId(int id);
    Task<TbTransactionMaster> PosMeFindByTransactionNumber(string transactionNumber);
	Task<List<TbTransactionMaster>> PosmeGetAll();
}