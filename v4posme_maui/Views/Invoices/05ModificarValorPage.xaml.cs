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
    private string Campo { get; set; }

    public void SetCampo(string name)
    {
        Campo = name;
    }
    public void SetQuantity(decimal value)
    {
        Quantity = value;
        Precio = decimal.Zero;
        TxtValor.Text = value.ToString("N2");
    }

    public void SetPrecio(decimal value)
    {
        Precio = value;
        Quantity = decimal.Zero;
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

      

        if (SelectedItem is null) 
            return;


        var value = decimal.Parse(valueText);
        //--wg-if (decimal.Compare(decimal.Zero, value) == 0)
        //--wg-{
        //--wg-    TxtValor.HasError = true;
        //--wg-    TxtValor.ErrorText = Mensajes.MensajeValorZero;
        //--wg-    return;
        //--wg-}

        if (decimal.Compare(Quantity, decimal.Zero) >= 0 && Campo == "cantidad" )
        {
            SelectedItem.Quantity = value;
        }

        if (decimal.Compare(Precio, decimal.Zero) >= 0 && Campo == "precio")
        {
            SelectedItem.PrecioPublico = value;
        }

        SelectedItem.Importe                    = decimal.Multiply(SelectedItem.Quantity, SelectedItem.PrecioPublico);
        VariablesGlobales.DtoInvoice.Balance    = VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe);

        var findIndex = VariablesGlobales.DtoInvoice.Items.FindIndex(response => response.ItemNumber == SelectedItem.ItemNumber);
        VariablesGlobales.DtoInvoice.Items.RemoveAt(findIndex);

        //Si la cantidad es mayor que 0 volver a insertar
        if(SelectedItem.Quantity > 0 )
        VariablesGlobales.DtoInvoice.Items.Insert(findIndex, SelectedItem);

        await Navigation.PopAsync(true);
    }
}