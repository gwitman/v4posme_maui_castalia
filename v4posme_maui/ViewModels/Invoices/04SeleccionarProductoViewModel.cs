using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Invoices;

public class SeleccionarProductoViewModel : BaseViewModel
{
    private readonly IRepositoryItems _repositoryItems;
    private readonly HelperCore _helper;

    public SeleccionarProductoViewModel()
    {
        Title                   = "Seleccionar producto 4/6";
        Productos               = new();
        _repositoryItems        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        AnadirProducto          = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnAnadirProducto);
        _helper                 = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        SearchBarCodeCommand    = new Command(OnSearchBarCode);
        SearchCommand           = new Command(OnSearch);
        ProductosSeleccionadosCommand = new Command(OnRevisarProductos);
    }

    private async void OnRevisarProductos(object obj)
    {
        if (VariablesGlobales.DtoInvoice.Items.Count<=0)
        {
            ShowToast(Mensajes.MensajeSeleccionarProductos, ToastDuration.Long,12);
            return;
        }
        IsBusy = true;
        await NavigationService.NavigateToAsync<RevisarProductosSeleccionadosViewModel>();
        IsBusy = false;
    }

    private async void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(Search))
        {
            IsPanelVisible = !IsPanelVisible;
            return;
        }

        IsPanelVisible = !IsPanelVisible;
        IsBusy = true;
        await Task.Run(async () =>
        {
            
            var valueTop    = await _helper.GetValueParameter("MOBILE_SHOW_TOP_ITEMS", "10");
            Productos.Clear();
            List<Api_AppMobileApi_GetDataDownloadItemsResponse> searchItems;

            if (string.IsNullOrWhiteSpace(Search))
            {
                searchItems = await _repositoryItems.PosMeDescending10(int.Parse(valueTop));
            }
            else
            {
                searchItems = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndName(Search);
            }

            foreach (var itemsResponse in searchItems)
            {
                itemsResponse.MonedaSimbolo = VariablesGlobales.DtoInvoice.Currency!.Simbolo;
                Productos.Add(itemsResponse);
            }
        });
        IsBusy = false;
    }

    private async void OnSearchBarCode()
    {
        var barCodePage     = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar             = await barCodePage.WaitForResultAsync();
        Search              = bar!;
        IsPanelVisible      = !IsPanelVisible;
    }

    private async void OnAnadirProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null)
        {
            return;
        }

        var permitirRepetidos           = await _helper.GetValueParameter("MOBILE_ALLOW_REPEATED_PRODUCTS", "false");
        var cestaArticulos              = VariablesGlobales.DtoInvoice.Items;
        var transactionMasterDetailID   = _helper.GetTimestampId();


        if ( (permitirRepetidos == "true")  )
        {
            // Agregar siempre como nueva línea independiente (clonar el ítem)
            var nuevo = new Api_AppMobileApi_GetDataDownloadItemsResponse
            {
                TransactionMasterDetailID 
                                    = transactionMasterDetailID,
                ItemPk              = obj.ItemPk,
                ItemId              = obj.ItemId,
                BarCode             = obj.BarCode,
                ItemNumber          = obj.ItemNumber,
                Name                = obj.Name,
                PrecioPublico       = obj.PrecioPublico,
                CantidadEntradas    = obj.CantidadEntradas,
                CantidadSalidas     = obj.CantidadSalidas,
                CantidadFinal       = obj.CantidadFinal,
                MonedaSimbolo       = obj.MonedaSimbolo,
                Quantity            = decimal.One,
                MontoDescuento      = 0m,
                PorcentajeDescuento = 0m
            };
            cestaArticulos.Add(nuevo);
        }
        else
        {
            var find = cestaArticulos.FirstOrDefault(response => response.ItemNumber == obj.ItemNumber);
            if (find is not null)
            {
                ShowToast(Mensajes.MensajeProductoYaAgregado, ToastDuration.Short, 12);
                return;
            }

            obj.Quantity        = decimal.One;
            obj.Importe         = obj.PrecioPublico;
            obj.MontoDescuento  = 0m;
            cestaArticulos.Add(obj);
        }

        VariablesGlobales.DtoInvoice.Balance    = VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe) - VariablesGlobales.DtoInvoice.Items.Sum(response => response.MontoDescuento);
        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada++;
        ProductosSeleccionadosCantidad          = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
        ProductosSeleccionadosCantidadTotal     = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";
    }

    private async void LoadProductos()
    {
        var valueTop        = await _helper.GetValueParameter("MOBILE_SHOW_TOP_ITEMS", "10");
        var findProductos   = await _repositoryItems.PosMeDescending10(int.Parse(valueTop));
        Productos.Clear();
        foreach (var itemsResponse in findProductos.OrderBy(p => p.Name ))
        {
            itemsResponse.MonedaSimbolo = VariablesGlobales.DtoInvoice.Currency!.Simbolo;
            Productos.Add(itemsResponse);
        }

        if (VariablesGlobales.DtoInvoice.Items.Count > 0)
        {
            ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
            ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";
        }

        IsBusy = false;
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        LoadProductos();
    }

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Productos { get; }

    private string _productosSeleccionadosCantidadTotal = "Items";

    public string ProductosSeleccionadosCantidadTotal
    {
        get => _productosSeleccionadosCantidadTotal;
        set => SetProperty(ref _productosSeleccionadosCantidadTotal, value);
    }

    private string _productosSeleccionadosCantidad = "Seleccionar Productos";

    public string ProductosSeleccionadosCantidad
    {
        get => _productosSeleccionadosCantidad;
        set => SetProperty(ref _productosSeleccionadosCantidad, value);
    }

    private int _cantidad;

    public int Cantidad
    {
        get => _cantidad;
        set => SetProperty(ref _cantidad, value);
    }

    public Command AnadirProducto { get; }
    public Command SearchCommand { get; }
    public Command SearchBarCodeCommand { get; }
    private bool _isPanelVisible;

    public bool IsPanelVisible
    {
        get => _isPanelVisible;
        set => SetProperty(ref _isPanelVisible, value);
    }

    public Command ProductosSeleccionadosCommand { get; }
}