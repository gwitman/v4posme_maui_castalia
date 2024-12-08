using System.Collections.ObjectModel;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Models;

public class ViewTempDtoInvoice
{
    public ViewTempDtoInvoice()
    {
        Items = new();
    }

    public Api_AppMobileApi_GetDataDownloadCustomerResponse? CustomerResponse { get; set; }

    public string? CustomerNumber { get; set; }

    public string? Identification { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? NombreCompleto
    {
        get => $"{FirstName} {LastName}";
    }

    public int CantidadTotalSeleccionada { get; set; }

    public decimal Balance { get; set; }

    public string? Comentarios { get; set; } = string.Empty;

    public string? Referencia { get; set; } = string.Empty;

    public DtoCatalogItem? Currency { get; set; }

    public DtoCatalogItem? TipoDocumento { get; set; }

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Items { get; }
    public string Codigo { get; set; }
    public decimal Monto { get; set; }
    public decimal Cambio { get; set; }
    public DateTime TransactionOn { get; set; }
    public int TransactionMasterId { get; set; }
    public TypePayment TipoPayment { get; set; }
    public decimal FixedExpenses { get; set; } = decimal.Zero;
    public int Plazo { get; set; } = 1;
    public DateTime NextVisit { get; set; } = DateTime.Now;
    public DtoCatalogItem PeriodPay { get; set; } 
    public TbTransactionMaster TransactionMaster { get; set; }

    public void ClearItems()
    {
        Items.Clear();
    }
}