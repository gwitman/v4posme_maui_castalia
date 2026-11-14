namespace v4posme_maui.Models;

// Respuesta de app_mobile_api/getDataLoadOrder (mismo "data" que app_invoice_billing/loadOrder de la web).
// Sin dependencias de MAUI: se enlaza en el proyecto de pruebas.
public class DtoLoadOrderResponse
{
    public bool Error { get; set; }
    public string Message { get; set; } = string.Empty;
    public DtoLoadOrderData? Data { get; set; }
}

public class DtoLoadOrderData
{
    public string? OrderNumber { get; set; }
    public int TransactionMasterID { get; set; }
    public string? ReferenceClientName { get; set; }
    public string? ReferenceClientIdentifier { get; set; }
    public string? NumberPhone { get; set; }
    public List<DtoLoadOrderItem> Items { get; set; } = [];
}

public class DtoLoadOrderItem
{
    public int ItemID { get; set; }
    public decimal Quantity { get; set; }
    // Precio de la solicitud: solo informativo. Igual que la web, se factura con el precio local del producto.
    public decimal Price { get; set; }
}
