using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.Invoices;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Invoices;

public class RevisarProductosSeleccionadosViewModel : BaseViewModel
{
    private readonly HelperCore _helper;

    public RevisarProductosSeleccionadosViewModel()
    {
        Title                   = "Productos Seleccionados 5/6";
        ProductosSeleccionados  = VariablesGlobales.DtoInvoice.Items;
        TapCommandProducto      = new Command(OnTapCommand);
        PrecioCommand           = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnPrecioCommand);
        QuantityCommand         = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnQuantityCommand);
        DescuentoCommand        = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnDescuentoCommand);
        ReferenciaCommand       = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnReferenciaCommand);
        PagoCommand             = new Command(OnPagoCommand);
        _helper                 = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
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
        modificarValorPage.SetQuantity(obj.Quantity, valorIngresado => { 
            RecalcularItem(obj);
            VariablesGlobales.DtoInvoice.Balance =
            VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe) -
            VariablesGlobales.DtoInvoice.Items.Sum(response => response.MontoDescuento);
        });
        modificarValorPage.SetCampo("cantidad");
        modificarValorPage.SetItemSelected(obj);
        Navigation!.PushAsync(modificarValorPage, true);
    }

    private void OnPrecioCommand(Api_AppMobileApi_GetDataDownloadItemsResponse obj)
    {
        var modificarValorPage = new ModificarValorPage();
        modificarValorPage.SetPrecio(obj.PrecioPublico, valorIngresado => { 
                RecalcularItem(obj);
                VariablesGlobales.DtoInvoice.Balance =
                VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe) -
                VariablesGlobales.DtoInvoice.Items.Sum(response => response.MontoDescuento);
        });
        modificarValorPage.SetCampo("precio");
        modificarValorPage.SetItemSelected(obj);
        Navigation!.PushAsync(modificarValorPage, true);
    }

    private void OnDescuentoCommand(Api_AppMobileApi_GetDataDownloadItemsResponse obj)
    {
        var modificarValorPage = new ModificarValorPage();
        modificarValorPage.SetCampo("descuento");
        modificarValorPage.SetItemSelected(obj);
        modificarValorPage.SetDescuento(obj.PorcentajeDescuento, valorIngresado =>
        {
            if (valorIngresado < 0 || valorIngresado > 100)
            {
                ShowToast(Mensajes.MensajeDescuentoFueraDeRango, ToastDuration.Short, 12);
            }
            RecalcularItem(obj);
            VariablesGlobales.DtoInvoice.Balance = 
                VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe) - 
                VariablesGlobales.DtoInvoice.Items.Sum(response => response.MontoDescuento);
        });
        Navigation!.PushAsync(modificarValorPage, true);
    }

    private void OnReferenciaCommand(Api_AppMobileApi_GetDataDownloadItemsResponse obj)
    {
        var modificarValorPage = new ModificarValorPage();
        modificarValorPage.SetCampo("referencia");
        modificarValorPage.SetItemSelected(obj);
        modificarValorPage.SetReferencia(obj.Referencia, valorIngresado =>
        {
            if (valorIngresado.Length > 255)
            {
                obj.Referencia = valorIngresado[..255];
                ShowToast(Mensajes.MensajeReferenciaTruncada, ToastDuration.Short, 12);
            }
            else
            {
                obj.Referencia = valorIngresado;
            }
        });
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
    public Command DescuentoCommand { get; }
    public Command ReferenciaCommand { get; }
    public Command PagoCommand { get; }


    private decimal _balance;

    public decimal Balance
    {
        get => ProductosSeleccionados.Sum(response => response.Importe) - ProductosSeleccionados.Sum(response => response.MontoDescuento);
        set => SetProperty(ref _balance, value);
    }

    private decimal _subTotal;

    public decimal SubTotal
    {
        get => ProductosSeleccionados.Sum(response => response.Importe);
        set => SetProperty(ref _subTotal, value);
    }

    public string MonedaSimbolo => VariablesGlobales.DtoInvoice.Currency is not null ? VariablesGlobales.DtoInvoice.Currency.Simbolo : "";

    public int CantidadTotalItems => (int)VariablesGlobales.DtoInvoice.Items.Sum(response => response.Quantity);

    private void RecalcularItem(Api_AppMobileApi_GetDataDownloadItemsResponse item)
    {
        // Clamp porcentaje a [0, 100]
        if (item.PorcentajeDescuento < 0) item.PorcentajeDescuento = 0;
        if (item.PorcentajeDescuento > 100) item.PorcentajeDescuento = 100;

        var precioTotal     = item.PrecioPublico * item.Quantity;
        item.MontoDescuento = precioTotal * item.PorcentajeDescuento / 100m;

        // Clamp monto a [0, precioTotal]
        if (item.MontoDescuento > precioTotal) item.MontoDescuento = precioTotal;
        if (item.MontoDescuento < 0) item.MontoDescuento = 0;

        OnPropertyChanged(nameof(SubTotal));
        OnPropertyChanged(nameof(Balance));
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation  = navigation;
        SubTotal    = ProductosSeleccionados.Sum(response => response.Importe);
        Balance     = ProductosSeleccionados.Sum(response => response.Importe) - ProductosSeleccionados.Sum(response => response.MontoDescuento);
        IsBusy      = false;
        _ = CargarParametros();
    }

    private async Task CargarParametros()
    {
        var value        = await _helper.GetValueParameter("MOBILE_IN_INVOICE_MOSTRAR_DESCUENTO", "false");
        MostrarDescuento = !(bool.TryParse(value, out var parsed) && parsed);
    }

    private bool _mostrarDescuento;
    public bool MostrarDescuento
    {
        get => _mostrarDescuento;
        set => SetProperty(ref _mostrarDescuento, value);
    }
}