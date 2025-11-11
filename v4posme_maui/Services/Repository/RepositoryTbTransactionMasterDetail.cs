using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryTbTransactionMasterDetail(DataBase dataBase) : RepositoryFacade<TbTransactionMasterDetail>(dataBase), IRepositoryTbTransactionMasterDetail
{
    private readonly DataBase _dataBase = dataBase;

    public Task<List<TbTransactionMasterDetail>> PosMeItemByTransactionId(int transactionId)
    {
        return _dataBase.Database.Table<TbTransactionMasterDetail>()
            .Where(
                detail => detail.TransactionMasterId == transactionId)
            .ToListAsync();
    }

    public Task<List<TbTransactionMasterDetail>> PosMeByTransactionIDAndItemID(int transactionId,int itemID)
    {
       

        var query = $"""
                     select 
                        tmd.TransactionMasterDetailId,                       
                        tmd.transactionMasterID as TransactionMasterId,
                        tmd.componentID as Componentid,
                        tmd.componentItemID as ComponentItemId,
                        tmd.quantity as Quantity,
                        tmd.unitaryCost as UnitaryCost,
                        tmd.UnitaryPrice as UnitaryPrice,
                        tmd.subAmount as SubAmount,
                        tmd.discount as Discount,
                        tmd.tax1 as Tax1,
                        tmd.amount as Amount,
                        tmd.itemBarCode as ItemBarCode,
                        tmd.Reference1 as Reference1,
                        tmd.Reference2 as Reference2
                     from 
                        tb_transaction_master_detail tmd
                        inner join tb_transaction_master tm on 
                            tm.transactionMasterID = tmd.transactionMasterID
                     where 
                        tm.transactionID = {transactionId} and 
                        tmd.componentItemID = {itemID}
                     """;

        return _dataBase.Database.QueryAsync<TbTransactionMasterDetail>(query);

    }

    public Task<List<TbTransactionMasterDetail>> PosMeByTransactionIDAndItemID_BetweenDate(int transactionId, int itemID,DateTime  startOn,DateTime endOn)
    {


        var query = @"
                     select 
                        tmd.TransactionMasterDetailId,                       
                        tmd.transactionMasterID as TransactionMasterId,
                        tmd.componentID as Componentid,
                        tmd.componentItemID as ComponentItemId,
                        tmd.quantity as Quantity,
                        tmd.unitaryCost as UnitaryCost,
                        tmd.UnitaryPrice as UnitaryPrice,
                        tmd.subAmount as SubAmount,
                        tmd.discount as Discount,
                        tmd.tax1 as Tax1,
                        tmd.amount as Amount,
                        tmd.itemBarCode as ItemBarCode,
                        tmd.Reference1 as Reference1,
                        tmd.Reference2 as Reference2
                     from 
                        tb_transaction_master_detail tmd
                        inner join tb_transaction_master tm on 
                            tm.transactionMasterID = tmd.transactionMasterID
                     where 
                        tm.transactionID = ? and 
                        tmd.componentItemID = ? and 
                        tm.TransactionOn between ? and ?  
                     ";

        
        return _dataBase.Database.QueryAsync<TbTransactionMasterDetail>(query, transactionId, itemID, startOn, endOn);

    }


}