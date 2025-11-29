using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using DevExpress.Maui.Core;
using DevExpress.Xpo.DB.Helpers;
using SQLite;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Models;

public class Api_AppMobileApi_GetDataDownloadResponse
{
    public bool Error { get; set; }
    public string? Message { get; set; }

    public TbCompany ObjCompany { get; set; } = new();
    public List<Api_AppMobileApi_GetDataDownloadMenuElementResponse> ListMenuElement { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadItemsResponse> ListItem { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadCustomerResponse> ListCustomer { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadParametersResponse> ListParameter { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadCatalogItemResponse> ListCatalogItem { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> ListDocumentCredit { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> ListDocumentCreditAmortization { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadServerTransactionMasterResponse> ListServerTransactionMaster { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadTransactionMasterRegisterResponse> ListTransactionMasterRegister { get; set; } = [];
}

[SQLite.Table("tb_server_transaction_master")]
public class Api_AppMobileApi_GetDataDownloadServerTransactionMasterResponse : BindableBase
{
    [PrimaryKey, AutoIncrement]
    [DataMember(Name = "transactionMasterID")]
    public int TransactionMasterID { get; set; }
    [DataMember(Name = "transactionID")] public int TransactionID { get => GetValue<int>(); set => SetValue(value); }
    [DataMember(Name = "currencyID")] public int CurrencyID { get => GetValue<int>(); set => SetValue(value); }
    [DataMember(Name = "amount")] public decimal Amount { get => GetValue<decimal>(); set => SetValue(value); }

}


[SQLite.Table("tb_customers")]
public class Api_AppMobileApi_GetDataDownloadCustomerResponse : BindableBase
{
    [PrimaryKey, AutoIncrement]
    [DataMember]
    public int CustomerId { get; set; }

    [DataMember(Name = "companyID")] public int CompanyId { get=>GetValue<int>(); set=>SetValue(value); }

    [DataMember(Name = "branchID")] public int BranchId { get=>GetValue<int>(); set=>SetValue(value); }

    [DataMember(Name = "entityID")] public int EntityId { get=>GetValue<int>(); set=>SetValue(value); }

    [DataMember(Name = "customerNumber")] public string? CustomerNumber { get=>GetValue<string>(); set=>SetValue(value); }

    [DataMember(Name = "identification")] public string? Identification { get=>GetValue<string>(); set=>SetValue(value); }

    [DataMember(Name = "firstName")] public string? FirstName { get=>GetValue<string>(); set=>SetValue(value); }

    [DataMember(Name = "lastName")] public string? LastName { get=>GetValue<string>(); set=>SetValue(value);}

    [DataMember(Name = "balance")] public decimal Balance { get=>GetValue<decimal>(); set=>SetValue(value); }

    [DataMember(Name = "currencyID")] public int CurrencyId { get=>GetValue<int>(); set=>SetValue(value); }

    [DataMember(Name="currencyName")] public string? CurrencyName { get=>GetValue<string>(); set=>SetValue(value); }

    [DataMember(Name = "customerCreditLineID")] public int CustomerCreditLineId { get=>GetValue<int>(); set=>SetValue(value); }
    
    [DataMember(Name = "location")] public string? Location { get=>GetValue<string>(); set=>SetValue(value); }
    
    [DataMember(Name = "phone")] public string? Phone { get=>GetValue<string>(); set=>SetValue(value); }
    
    [DataMember(Name = "me")] public int Me { get=>GetValue<int>(); set=>SetValue(value); }
    
    public bool Modificado { get=>GetValue<bool>(); set=>SetValue(value); }
    
    public int SecuenciaAbono { get=>GetValue<int>(); set=>SetValue(value); }
    
    public int? Secuencia { get=>GetValue<int>(); set=>SetValue(value); }

    [DataMember(Name = "ordenAbono")]
    public int OrdenAbono { get=>GetValue<int>(); set=>SetValue(value); }

    [DataMember(Name = "isHaveShareNow")]
    public int IsHaveShareNow { get => GetValue<int>(); set => SetValue(value); }

    [DataMember(Name = "frecuencyNameIntoShare")]
    public string FrecuencyNameIntoShare { get => GetValue<string>(); set => SetValue(value); }

    [DataMember(Name = "showFrecuenciInCustomerIntoShare")]
    public int ShowFrecuenciInCustomerIntoShare { get => GetValue<int>(); set => SetValue(value); }

    [NotMapped] public string? NombreCompleto => $"{FirstName} {LastName}";
    
    [NotMapped] public decimal? Remaining { get; set; }
    [NotMapped] public bool HasAbono {get=>GetValue<bool>(); set=>SetValue(value); }
    [NotMapped] public bool Asignado => Me == 1;
    [NotMapped] public bool Facturado { get; set; }
    [NotMapped] public DateTime FirstBalanceDate { get=>GetValue<DateTime>(); set=>SetValue(value); }

  
}

[SQLite.Table("tb_company")]
public class TbCompany
{
    [PrimaryKey]public int CompanyId { get; set; }
    public string? Name { get; set; }
    public DateTime CreatedOn { get; set; }
    public string? Address { get; set; }
    public int FlavorId { get; set; }
    public string? Type { get; set; }
}

[SQLite.Table("tb_document_credit")]
public class Api_AppMobileApi_GetDataDownloadDocumentCreditResponse
{
    [PrimaryKey] public int CustomerCreditDocumentId { get; set; }
    public int EntityId { get; set; }
    public int CustomerCreditLineId { get; set; }
    public string? DocumentNumber { get; set; }
    public string? CurrencyName { get; set; }
    public int CurrencyId { get; set; }
    public int StatusDocument { get; set; }
    public int CreditAmortizationId { get; set; }
    public DateTime DateApply { get; set; }
    public decimal Remaining { get; set; }
    public int StatusAmortization { get; set; }
    public string? StatusAmortizationName { get; set; }
    public decimal Balance { get; set; }
    public decimal ExchangeRate { get; set; }
    public decimal CuotaPactada { get; set; }
    public int CantidadCuotas { get; set; }
}

[SQLite.Table("document_credit_amortization")]
public class Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse
{
    [PrimaryKey] public int CreditAmortizationId { get; set; }
    
    public string? CustomerNumber { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateTime BirthDate { get; set; }

    public string? DocumentNumber { get; set; }

    [DataMember(Name = "currencyID")] 
    public TypeCurrency CurrencyId { get; set; }

    public string? ReportSinRiesgo { get; set; }

    public DateTime DateApply { get; set; }

    public decimal Remaining { get; set; }

    public decimal Balance { get; set; }

    public int Sequence { get; set; }
    
    [NotMapped] public string? CurrencyName { get; set; }
    [NotMapped] public decimal MontoCuota { get; set; }
}

[SQLite.Table("tb_menu_element")]
public class Api_AppMobileApi_GetDataDownloadMenuElementResponse : BindableBase
{
    [PrimaryKey]
    [DataMember(Name = "menuElementID")]
    public int MenuElementId
    {
        get => GetValue<int>();
        set => SetValue(value);

    }
    [DataMember(Name = "display")]
    public string Display
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    [DataMember(Name = "address")]
    public string Address
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    [DataMember(Name = "icon")]
    public string Icon
    {
        get => GetValue<string>();
        set => SetValue(value);
    }
    [DataMember(Name = "typeMenuElementID")]
    public int TypeMenuElementID
    {
        get => GetValue<int>();
        set => SetValue(value);
    }
    [DataMember(Name = "selected")]
    public int Selected
    {
        get => GetValue<int>();
        set => SetValue(value);
    }
    [DataMember(Name = "inserted")]
    public int Inserted
    {
        get => GetValue<int>();
        set => SetValue(value);
    }
    [DataMember(Name = "deleted")]
    public int Deleted
    {
        get => GetValue<int>();
        set => SetValue(value);
    }
    [DataMember(Name = "edited")]
    public int Edited
    {
        get => GetValue<int>();
        set => SetValue(value);
    }
}

[SQLite.Table("tb_items")]
public class Api_AppMobileApi_GetDataDownloadItemsResponse : BindableBase
{
    [PrimaryKey, AutoIncrement]
    public int ItemPk
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    [DataMember(Name = "itemID")]
    public int ItemId
    {
        get => GetValue<int>();
        set => SetValue(value);
    }

    [DataMember(Name = "barCode")]
    public string BarCode
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [DataMember(Name = "itemNumber")]
    public string? ItemNumber
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [DataMember(Name = "name")]
    public string Name
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [DataMember(Name ="quantity")]
    public decimal Quantity
    {
        get => GetValue<decimal>();
        set => SetValue(value);
    }

    [DataMember(Name = "precioPublico")]
    public decimal PrecioPublico
    {
        get => GetValue<decimal>();
        set => SetValue(value);
    }
    [DataMember(Name = "cantidadEntradas")]
    public decimal CantidadEntradas
    {
        get => GetValue<decimal>();
        set => SetValue(value);
    }

    [DataMember(Name = "cantidadSalidas")]
    public decimal CantidadSalidas
    {
        get => GetValue<decimal>();
        set => SetValue(value);
    }

    [DataMember(Name = "cantidadFinal")]
    public decimal CantidadFinal
    {
        get => GetValue<decimal>();
        set => SetValue(value);
    }
    
    public bool Modificado { get; set; }

    /*
     * Se usa para cuando se selecciona en el grid de la factur y mostrar más detalles
     * del producto, como cantidad, precio unitario y descuento
     */
    [NotMapped]
    public bool IsSelected
    {
        get => GetValue<bool>();
        set => SetValue(value);
    }

    [NotMapped]
    public string MonedaSimbolo
    {
        get => GetValue<string>();
        set => SetValue(value);
    }

    [NotMapped]
    public decimal Importe
    {
        get => GetValue<decimal>();
        set => SetValue(value);
    }

    [NotMapped]
    public decimal CantidadFacturadas
    {
        get => GetValue<decimal>();
        set => SetValue(value);
    }
}

[SQLite.Table("tb_parameters")]
public class Api_AppMobileApi_GetDataDownloadParametersResponse
{
    [PrimaryKey, AutoIncrement] public int ParametersPk { get; set; }

    public int ComapnyId { get; set; }

    public int ParameterId { get; set; }

    public string? Display { get; set; }

    public string? Description { get; set; }

    public string? Value { get; set; }

    public string? CustomValue { get; set; }

    public string? Name { get; set; }
}



[SQLite.Table("tb_catalog_item")]
public class Api_AppMobileApi_GetDataDownloadCatalogItemResponse
{
    public int catalogID { get; set; }

    public string catalogName { get; set; }

    public string catalogDescription { get; set; }

    public int catalogItemID { get; set; }

    public string catalogItemName { get; set; }

    public string catalogItemDescriptionName { get; set; }

    public string catalogItemDisplay { get; set; }
    public decimal ratio { get; set; }
    public string? reference1 { get; set; }
    public string? reference2 { get; set; }
}


public class Api_AppMobileApi_GetDataDownloadTransactionMasterRegisterResponse
{

    // ------------------------------
    // CAMPOS DE transaction_master (c.)
    // ------------------------------
    public string? tm_transactionMasterMobileID { get; set; }
    public string? tm_transactionMasterMobileNumber { get; set; }

    public int tm_transactionMasterID { get; set; }
    public int tm_transactionID { get; set; }
    public string? tm_transactionNumber { get; set; }
    public int tm_entityID { get; set; }
    public int tm_entityIDSecondary { get; set; }
    public DateTime tm_transactionOn { get; set; }
    public DateTime tm_transactionOn2 { get; set; }
    public DateTime? tm_nextVisit { get; set; }
    public int tm_statusID { get; set; }

    public int tm_mesaID { get; set; }
    public string? tm_mesaName { get; set; }
    public string? tm_referenceClientIdentifier { get; set; }
    public string? tm_referenceClientName { get; set; }

    public int tm_plazo { get; set; }
    public string? tm_reference1 { get; set; }
    public string? tm_reference3 { get; set; }
    public string? tm_note { get; set; }

    public int tm_currencyID { get; set; }
    public decimal tm_exchangeRate { get; set; }
    public int tm_transactionCausalID { get; set; }

    public decimal tm_Amount { get; set; }
    public decimal tm_subAmount { get; set; }
    public decimal tm_tax1 { get; set; }
    public decimal tm_discount { get; set; }

    public int tm_customerCreditLineID { get; set; }
    public int tm_periodPay { get; set; }
    public decimal tm_fixedExpenses { get; set; }

    // ------------------------------
    // CAMPOS DE transaction_master_detail (tmd.)
    // ------------------------------
    public int tmd_transactionMasterDetailID { get; set; }
    public int tmd_componentID { get; set; }
    public int tmd_componentItemID { get; set; }
    public decimal tmd_quantity { get; set; }
    public decimal tmd_unitaryCost { get; set; }
    public decimal tmd_unitaryPrice { get; set; }
    public decimal tmd_SubAmount { get; set; }
    public decimal tmd_Amount { get; set; }
    public decimal tmd_Discount { get; set; }
    public decimal tmd_tax1 { get; set; }

    // ------------------------------
    // CAMPOS DE items (i.)
    // ------------------------------
    public string? tmd_itemNumber { get; set; }
    public string? tmd_itemName { get; set; }
    public string? tmd_barCode { get; set; }

    public string? tmd_reference1 { get; set; }
    public string? tmd_reference2 { get; set; }
}