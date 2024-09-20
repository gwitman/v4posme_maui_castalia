using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using DevExpress.Maui.Core;
using SQLite;

namespace v4posme_maui.Models;

public class Api_AppMobileApi_GetDataDownloadResponse
{
    public bool Error { get; set; }
    public string? Message { get; set; }

    public TbCompany ObjCompany { get; set; } = new();
    public List<Api_AppMobileApi_GetDataDownloadItemsResponse> ListItem { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadCustomerResponse> ListCustomer { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadParametersResponse> ListParameter { get; set; } = [];
    public List<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> ListDocumentCredit { get; set; } = [];

    public List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> ListDocumentCreditAmortization { get; set; } = [];
}

[SQLite.Table("tb_customers")]
public class Api_AppMobileApi_GetDataDownloadCustomerResponse : BindableBase
{
    [PrimaryKey, AutoIncrement]
    [DataMember]
    public int CustomerId { get; set; }

    [DataMember(Name = "companyID")] public int CompanyId { get; set; }

    [DataMember(Name = "branchID")] public int BranchId { get; set; }

    [DataMember(Name = "entityID")] public int EntityId { get; set; }

    [DataMember(Name = "customerNumber")] public string? CustomerNumber { get; set; }

    [DataMember(Name = "identification")] public string? Identification { get; set; }

    [DataMember(Name = "firstName")] public string? FirstName { get; set; }

    [DataMember(Name = "lastName")] public string? LastName { get; set; }

    [DataMember(Name = "balance")] public decimal Balance { get; set; }

    [DataMember(Name = "currencyID")] public int CurrencyId { get; set; }

    [DataMember(Name="currencyName")] public string? CurrencyName { get; set; }

    [DataMember(Name = "customerCreditLineID")] public int CustomerCreditLineId { get; set; }
    public bool Modificado { get; set; }

    [NotMapped] public string? NombreCompleto => $"{FirstName} {LastName}";
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

    public int CurrencyId { get; set; }

    public string? ReportSinRiesgo { get; set; }

    public DateTime DateApply { get; set; }

    public decimal Remaining { get; set; }

    public decimal Balance { get; set; }
    
    [NotMapped] public string CurrencyName { get; set; }
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