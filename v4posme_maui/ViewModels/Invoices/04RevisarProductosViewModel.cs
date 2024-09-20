using System.Collections.ObjectModel;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.Invoices;

namespace v4posme_maui.ViewModels.Invoices;

public class RevisarProductosSeleccionadosViewModel : BaseViewModel
{
    public RevisarProductosSeleccionadosViewModel()
    {
        Title = "Productos Seleccionados 4/5";
        ProductosSeleccionados = VariablesGlobales.DtoInvoice.Items;
        TapCommandProducto = new Command(OnTapCommand);
        PrecioCommand = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnPrecioCommand);
        QuantityCommand = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnQuantityCommand);
        PagoCommand = new Command(OnPagoCommand);
    }

    private async void OnPagoCommand()
    {
        IsBusy = true;
        await NavigationService.NavigateToAsync<PaymentInvoiceViewModel>();
        IsBusy = false;
    }

    private void OnQuantityCommand(Api_AppMobileApi_GetDataDownloadItemsResponse obj)
    {
        var modificarValorPage = new ModificarValorPage();
        modificarValorPage.SetQuantity(obj.Quantity);
        modificarValorPage.SetItemSelected(obj);
        Navigation!.PushAsync(modificarValorPage, true);
    }

    private void OnPrecioCommand(Api_AppMobileApi_GetDataDownloadItemsResponse obj)
    {
        var modificarValorPage = new ModificarValorPage();
        modificarValorPage.SetPrecio(obj.PrecioPublico);
        modificarValorPage.SetItemSelected(obj);
        Navigation!.PushAsync(modificarValorPage, true);
    }

    private void OnTapCommand()
    {
        foreach (var seleccionado in ProductosSeleccionados)
        {
            seleccionado.IsSelected = false;
        }

        if (SelectedItem is not null)
        {
            SelectedItem.IsSelected = !SelectedItem.IsSelected;
        }
    }

    private ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> _productosSeleccionados = new();

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> ProductosSeleccionados
    {
        get => _productosSeleccionados;
        set
        {
            SetProperty(ref _productosSeleccionados, value);
            OnPropertyChanged(nameof(SubTotal));
        }
    }

    public Api_AppMobileApi_GetDataDownloadItemsResponse? SelectedItem { get; set; }
    public Command TapCommandProducto { get; }
    public Command PrecioCommand { get; }
    public Command QuantityCommand { get; }
    public Command PagoCommand { get; }

    private decimal _subTotal;

    public decimal SubTotal
    {
        get => ProductosSeleccionados.Sum(response => response.Importe);
        set => SetProperty(ref _subTotal, value);
    }

    public string MonedaSimbolo => VariablesGlobales.DtoInvoice.Currency is not null ? VariablesGlobales.DtoInvoice.Currency.Simbolo : "";

    public int CantidadTotalItems => (int)VariablesGlobales.DtoInvoice.Items.Sum(response => response.Quantity);

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        SubTotal = ProductosSeleccionados.Sum(response => response.Importe);
        IsBusy = false;
    }
}