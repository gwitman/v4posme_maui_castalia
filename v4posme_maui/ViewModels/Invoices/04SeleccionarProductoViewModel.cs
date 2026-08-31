using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core;
using DevExpress.Maui.Core.Internal;
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
    private int _loadBatchSize = 10;
    private int _lastLoadedIndex;
    private bool _hasMoreItems = true;
    private bool _isLoadingMore;

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
        LoadMoreCommand         = new Command(OnLoadMore);
        QuitarProductoCommand   = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnQuitarProducto);
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

    private void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(Search))
        {
            IsPanelVisible = !IsPanelVisible;
            return;
        }

        IsPanelVisible      = !IsPanelVisible;
        _lastLoadedIndex    = 0;
        _hasMoreItems       = true;
        Productos.Clear();
        LoadProductosBatch();
    }

    private void OnLoadMore()
    {
        if (_isLoadingMore || !_hasMoreItems) return;
        _isLoadingMore = true;
        LoadProductosBatch();
    }

    private async void LoadProductosBatch()
    {
        await Task.Run(async () =>
        {
            Thread.Sleep(1000);
            List<Api_AppMobileApi_GetDataDownloadItemsResponse> items;

            if (string.IsNullOrWhiteSpace(Search))
            {
                items = await _repositoryItems.PosMeAscBySizeAndTop(_lastLoadedIndex, _loadBatchSize);
            }
            else
            {
                items = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndNameByTop(Search, _lastLoadedIndex, _loadBatchSize);
            }

            if (items.Count < _loadBatchSize)
            {
                _hasMoreItems = false;
            }

            foreach (var item in items)
            {
                item.MonedaSimbolo = VariablesGlobales.DtoInvoice.Currency!.Simbolo;
            }

            _lastLoadedIndex += items.Count;
            Productos.AddRange(items);
            _isLoadingMore = false;
            IsBusy = false;
        });
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
                find.Quantity       += decimal.One;
                find.Importe        = find.PrecioPublico * find.Quantity;
                find.MontoDescuento = find.PorcentajeDescuento > 0 
                    ? find.Importe * (find.PorcentajeDescuento / 100m) 
                    : find.MontoDescuento;
            }
            else
            {
                obj.TransactionMasterDetailID   = transactionMasterDetailID;
                obj.Quantity                    = decimal.One;
                obj.Importe                     = obj.PrecioPublico;
                obj.MontoDescuento              = 0m;
                cestaArticulos.Add(obj);
            }
        }

        VariablesGlobales.DtoInvoice.Balance    = VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe) - VariablesGlobales.DtoInvoice.Items.Sum(response => response.MontoDescuento);
        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada++;
        ProductosSeleccionadosCantidad          = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
        ProductosSeleccionadosCantidadTotal     = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";
    }

    private void OnQuitarProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        var cestaArticulos  = VariablesGlobales.DtoInvoice.Items;
        var find            = cestaArticulos.FirstOrDefault(response => response.ItemNumber == obj.ItemNumber);
        if (find is null) return;

        if (find.Quantity > decimal.One)
        {
            find.Quantity       -= decimal.One;
            find.Importe        = find.PrecioPublico * find.Quantity;
            find.MontoDescuento = find.PorcentajeDescuento > 0
                ? find.Importe * (find.PorcentajeDescuento / 100m)
                : find.MontoDescuento;
        }
        else
        {
            cestaArticulos.Remove(find);
        }

        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada  = cestaArticulos.Count;
        VariablesGlobales.DtoInvoice.Balance                    = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);
        ProductosSeleccionadosCantidad                          = cestaArticulos.Count > 0
            ? $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items"
            : "Seleccionar Productos";
        ProductosSeleccionadosCantidadTotal                     = cestaArticulos.Count > 0
            ? $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}"
            : "Items";
    }

    private async void LoadProductos()
    {
        var valueTop = await _helper.GetValueParameter("MOBILE_SHOW_TOP_ITEMS", "10");
        _loadBatchSize = int.Parse(valueTop);
        _lastLoadedIndex = 0;
        _hasMoreItems = true;
        Productos.Clear();
        LoadProductosBatch();

        if (VariablesGlobales.DtoInvoice.Items.Count > 0)
        {
            ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
            ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";
        }
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        LoadProductos();
    }

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Productos { get; }

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
    public Command LoadMoreCommand { get; }
    public Command<Api_AppMobileApi_GetDataDownloadItemsResponse> QuitarProductoCommand { get; }
    private bool _isPanelVisible;

    public bool IsPanelVisible
    {
        get => _isPanelVisible;
        set => SetProperty(ref _isPanelVisible, value);
    }

    public Command ProductosSeleccionadosCommand { get; }
}
