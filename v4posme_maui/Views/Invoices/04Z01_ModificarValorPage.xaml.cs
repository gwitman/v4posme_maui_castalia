using DevExpress.Data.Extensions;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Views.Invoices;

public partial class ModificarValorPage : ContentPage
{
    public ModificarValorPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        TxtValor.Focus();
    }

    private decimal Quantity { get; set; }
    private decimal Precio { get; set; }

    public void SetQuantity(decimal value)
    {
        Quantity = value;
        Precio = decimal.Zero;
        TxtValor.Text = value.ToString("N2");
    }

    public void SetPrecio(decimal value)
    {
        Precio = decimal.Zero;
        Precio = value;
        TxtValor.Text = value.ToString("N2");
    }

    public void SetItemSelected(Api_AppMobileApi_GetDataDownloadItemsResponse itemsResponse)
    {
        SelectedItem = itemsResponse;
    }

    private Api_AppMobileApi_GetDataDownloadItemsResponse? SelectedItem { get; set; }

    private async void DXButtonBase_OnClicked(object? sender, EventArgs e)
    {
        var valueText = TxtValor.Text;
        if (string.IsNullOrWhiteSpace(valueText))
        {
            TxtValor.HasError = true;
            return;
        }

        var value = decimal.Parse(valueText);
        if (decimal.Compare(decimal.Zero, value)==0)
        {
            TxtValor.HasError = true;
            TxtValor.ErrorText = Mensajes.MensajeValorZero;
            return;
        }
        if (SelectedItem is null) return;
        if (decimal.Compare(Quantity, decimal.Zero) > 0)
        {
            SelectedItem.Quantity = value;
        }

        if (decimal.Compare(Precio, decimal.Zero) > 0)
        {
            SelectedItem.PrecioPublico = value;
        }

        SelectedItem.Importe = decimal.Multiply(SelectedItem.Quantity, SelectedItem.PrecioPublico);
        VariablesGlobales.DtoInvoice.Balance = VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe);
        var findIndex = VariablesGlobales.DtoInvoice.Items.FindIndex(response => response.ItemNumber == SelectedItem.ItemNumber);
        VariablesGlobales.DtoInvoice.Items.RemoveAt(findIndex);
        VariablesGlobales.DtoInvoice.Items.Insert(findIndex, SelectedItem);
        await Navigation.PopAsync(true);
    }
}