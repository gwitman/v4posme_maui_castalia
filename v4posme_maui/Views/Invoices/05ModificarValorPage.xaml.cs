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
        // Delay needed so the control is fully rendered before focusing/selecting
        Dispatcher.DispatchAsync(async () =>
        {
            await Task.Delay(300);
            TxtValor.Focus();
            TxtValor.SelectAll();
        });
    }

    private decimal Quantity { get; set; }
    private decimal Precio { get; set; }
    private string Campo { get; set; }
    private Action<decimal>? OnDescuentoConfirmado { get; set; }
    private Action<string>? OnReferenciaConfirmada { get; set; }
    private Action<decimal>? OnQuantityConfirmado { get; set; }
    private Action<decimal>? OnPriceConfirmada { get; set; }

    public void SetCampo(string name)
    {
        Campo = name;
    }
    public void SetQuantity(decimal value, Action<decimal> onConfirmado)
    {
        OnQuantityConfirmado = onConfirmado;
        Quantity = value;
        Precio = decimal.Zero;
        TxtValor.Text = value.ToString("N2");
    }

    public void SetPrecio(decimal value, Action<decimal> onConfirmado)
    {
        OnPriceConfirmada = onConfirmado;
        Precio = value;
        Quantity = decimal.Zero;
        TxtValor.Text = value.ToString("N2");
    }

    public void SetDescuento(decimal value, Action<decimal> onConfirmado)
    {
        OnDescuentoConfirmado = onConfirmado;
        TxtValor.Text = value.ToString("N2");
    }

    public void SetReferencia(string value, Action<string> onConfirmada)
    {
        OnReferenciaConfirmada = onConfirmada;
        TxtValor.Text = value ?? string.Empty;
        TxtValor.Keyboard = Keyboard.Default;
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

        if (Campo == "descuento")
        {
            if (!decimal.TryParse(valueText, out var descuento))
            {
                TxtValor.HasError = true;
                return;
            }
            SelectedItem.PorcentajeDescuento = descuento;
            OnDescuentoConfirmado?.Invoke(descuento);
            await Navigation.PopAsync(true);
            return;
        }

        if (Campo == "referencia")
        {
            SelectedItem.Referencia = valueText;
            OnReferenciaConfirmada?.Invoke(valueText);
            await Navigation.PopAsync(true);
            return;
        }

        var value = decimal.Parse(valueText);

        if (decimal.Compare(Quantity, decimal.Zero) >= 0 && Campo == "cantidad")
        {
            SelectedItem.Quantity = value;
        }

        if (decimal.Compare(Precio, decimal.Zero) >= 0 && Campo == "precio")
        {
            SelectedItem.PrecioPublico = value;
        }

        SelectedItem.Importe                    = decimal.Multiply(SelectedItem.Quantity, SelectedItem.PrecioPublico);
        VariablesGlobales.DtoInvoice.Balance    = VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe) - VariablesGlobales.DtoInvoice.Items.Sum(response => response.MontoDescuento);

        var findIndex = VariablesGlobales.DtoInvoice.Items.FindIndex(response => response.ItemNumber == SelectedItem.ItemNumber);
        VariablesGlobales.DtoInvoice.Items.RemoveAt(findIndex);

        //Si la cantidad es mayor que 0 volver a insertar
        if (SelectedItem.Quantity > 0)
            VariablesGlobales.DtoInvoice.Items.Insert(findIndex, SelectedItem);


        if (Campo == "precio")
        {
            OnPriceConfirmada?.Invoke(value);
        }
        if (Campo == "cantidad")
        { 
            OnQuantityConfirmado?.Invoke(value);
        }
        await Navigation.PopAsync(true);
    }
}