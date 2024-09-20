using System.Runtime.Serialization;
using SQLite;

namespace v4posme_maui.Models;

[Table("tb_transaction_master_detail")]
public class TbTransactionMasterDetail
{
    [PrimaryKey, AutoIncrement] public int TransactionMasterDetailId { get; set; }

    [DataMember(Name = "transactionMasterID")]
    public int TransactionMasterId { get; set; }

    [DataMember(Name = "componentID")] public int Componentid { get; set; }

    [DataMember(Name = "componentItemID")] public int ComponentItemId { get; set; }

    [DataMember(Name = "quantity")] public decimal Quantity { get; set; }

    [DataMember(Name = "unitaryCost")] public decimal UnitaryCost { get; set; }

    [DataMember(Name = "unityPrice")] public decimal UnitaryPrice { get; set; }

    [DataMember(Name = "subAmount")] public decimal SubAmount { get; set; }

    [DataMember(Name = "discount")] public decimal Discount { get; set; }

    [DataMember(Name = "tax1")] public decimal Tax1 { get; set; }

    [DataMember(Name = "amount")] public decimal Amount { get; set; }

    [DataMember(Name = "itemBarCode")] public string ItemBarCode { get; set; } = string.Empty;

    [DataMember(Name = "referencie1")] public string Reference1 { get; set; } = string.Empty;

    [DataMember(Name = "referencie2")] public string Reference2 { get; set; } = string.Empty;
}