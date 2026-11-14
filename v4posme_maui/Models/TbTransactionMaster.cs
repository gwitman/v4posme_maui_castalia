using System.Runtime.Serialization;
using v4posme_maui.Services.Helpers;
using SQLite;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Models;

[Table("tb_transaction_master")]
public class TbTransactionMaster
{
    [DataMember(Name = "transactionID")] public TypeTransaction TransactionId { get; set; }
    [DataMember(Name = "typePaymentID")] public TypePayment TypePaymentId { get; set; } = TypePayment.Efectivo;

    [DataMember(Name = "transactionMasterID")]
    [PrimaryKey, AutoIncrement]
    public int TransactionMasterId { get; set; }

    [DataMember(Name = "transactionNumber")]
    public string? TransactionNumber { get; set; }

    [DataMember(Name = "entityID")] public int EntityId { get; set; }
    [DataMember(Name = "transactionOn")] public DateTime TransactionOn { get; set; }
    [DataMember(Name = "transactionOn2")] public DateTime TransactionOn2 { get; set; }
    [DataMember(Name = "nextVisit")] public DateTime NextVisit { get; set; } = DateTime.Now.Date;
    [DataMember(Name = "plazo")] public int Plazo { get; set; } = 1;
    [DataMember(Name = "fixedExpenses")] public decimal FixedExpenses { get; set; } = decimal.Zero;
    [DataMember(Name = "periodPay")] public TypePeriodPay PeriodPay { get; set; } = TypePeriodPay.Mensual;

    [DataMember(Name = "entitySecondaryID")]
    public string? EntitySecondaryId { get; set; }

    [DataMember(Name = "subAmount")] public decimal SubAmount { get; set; }
    [DataMember(Name = "discount")] public decimal Discount { get; set; }
    [DataMember(Name = "tax1")] public decimal Taxi1 { get; set; }
    [DataMember(Name = "amount")] public decimal Amount { get; set; }

    [DataMember(Name = "customerCreditLineID")]
    public int CustomerCreditLineId { get; set; }

    [DataMember(Name = "transactionCausalID")]
    public TypeTransactionCausal TransactionCausalId { get; set; }

    [DataMember(Name = "exchangeRate")] public decimal ExchangeRate { get; set; }
    [DataMember(Name = "currencyID")] public TypeCurrency CurrencyId { get; set; }

    [DataMember(Name = "comment")] public string? Comment { get; set; } = string.Empty;

    [DataMember(Name = "referencie1")] public string? Reference1 { get; set; } = string.Empty;
    [DataMember(Name = "referencie2")] public string? Reference2 { get; set; } = string.Empty;
    [DataMember(Name = "referencie3")] public string? Reference3 { get; set; } = string.Empty;
    [DataMember(Name = "CustomerIdentification")] public string CustomerIdentification { get; set; } = string.Empty;
    [DataMember(Name = "ReferenceClientName")] public string ReferenceClientName { get; set; } = string.Empty;
    [DataMember(Name = "MesaID")] public int MesaID { get; set; } = 1;
    [DataMember(Name = "MesaName")] public string MesaName { get; set; } = string.Empty;
    // Datos adicionales de facturacion (equivalentes a la factura web). Opcionales:
    // si van vacios / 0 el servidor conserva su comportamiento anterior.
    [DataMember(Name = "NumberPhone")] public string NumberPhone { get; set; } = string.Empty;                              // tb_transaction_master.numberPhone
    [DataMember(Name = "ReferenceClientIdentifier")] public string ReferenceClientIdentifier { get; set; } = string.Empty;  // tb_transaction_master_info.referenceClientIdentifier
    [DataMember(Name = "ReferenceInvoice")] public string ReferenceInvoice { get; set; } = string.Empty;                    // tb_transaction_master.reference3 (Reference3 movil ya se usa para saldo nuevo)
    [DataMember(Name = "ZoneID")] public int ZoneID { get; set; } = 0;                                                      // tb_transaction_master_info.zoneID
    [DataMember(Name = "ZoneName")] public string ZoneName { get; set; } = string.Empty;
    [DataMember(Name = "StatusID")] public int StatusID { get; set; } = 1;

    [DataMember(Name = "referencie4")] public string? Reference4 { get; set; }
    public int CuotasPendientes { get; set; }
    public int RegisterLocal { get; set; } = 0;
}