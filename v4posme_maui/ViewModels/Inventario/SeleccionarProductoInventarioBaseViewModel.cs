using DevExpress.Maui.Core;
using DevExpress.Maui.Core.Internal;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Inventario;

// Base compartida para la seleccion de productos en los flujos de inventario
// (Entrada y Salida). Reproduce el comportamiento de la pantalla 4/6 de facturacion:
// - clic sobre el producto lo agrega (o incrementa cantidad)
// - deslizar a la izquierda disminuye la cantidad / lo elimina
// - boton inferior para avanzar a la revision
public abstract class SeleccionarProductoInventarioBaseViewModel : BaseViewModel
{
    protected readonly IRepositoryItems RepositoryItems;
    protected readonly HelperCore Helper;

    protected SeleccionarProductoInventarioBaseViewModel()
    {
        Productos                     = new();
        RepositoryItems               = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        Helper                        = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        AnadirProducto                = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnAnadirProducto);
        QuitarProductoCommand         = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnQuitarProducto);
        SearchCommand                 = new Command(OnSearch);
        SearchBarCodeCommand          = new Command(OnSearchBarCode);
        ProductosSeleccionadosCommand = new Command(OnRevisarProductos);
        AtrasCommand                  = new Command(OnAtras);
    }

    // Navegacion al paso de revision (implementada por cada flujo concreto).
    protected abstract Task NavegarARevisarAsync();

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Productos { get; }

    public Command AnadirProducto { get; }
    public Command<Api_AppMobileApi_GetDataDownloadItemsResponse> QuitarProductoCommand { get; }
    public Command SearchCommand { get; }
    public Command SearchBarCodeCommand { get; }
    public Command ProductosSeleccionadosCommand { get; }
    public Command AtrasCommand { get; }

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

    private bool _isPanelVisible;
    public bool IsPanelVisible
    {
        get => _isPanelVisible;
        set => SetProperty(ref _isPanelVisible, value);
    }

    private async void OnAtras()
    {
        await NavigationService.GoBackAsync();
    }

    private async void OnSearch()
    {
        // Busca directamente con el texto actual de la barra de busqueda superior.
        await LoadAllProductosAsync();
    }

    private async void OnSearchBarCode()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar         = await barCodePage.WaitForResultAsync();
        Search          = bar!;
        await LoadAllProductosAsync();
    }

    protected async Task LoadAllProductosAsync()
    {
        IsBusy = true;
        try
        {
            List<Api_AppMobileApi_GetDataDownloadItemsResponse> items;
            if (string.IsNullOrWhiteSpace(Search))
                items = await RepositoryItems.PosMeFindAll();
            else
                items = await RepositoryItems.PosMeFilterdByItemNumberAndBarCodeAndName(Search);

            items = items.OrderBy(i => i.Name).ToList();
            foreach (var item in items)
            {
                item.Name          = item.Name?.ToLower();
                item.MonedaSimbolo = "C$";
            }

            Productos.Clear();
            Productos.AddRange(items);
        }
        catch (Exception ex)
        {
            ShowToast($"Error al cargar productos: {ex.Message}", ToastDuration.Long, 12);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnAnadirProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        var cesta = VariablesGlobales.DtoInventario.Items;
        var find  = cesta.FirstOrDefault(response => response.ItemNumber == obj.ItemNumber);
        if (find is not null)
        {
            find.Quantity += decimal.One;
            find.Importe   = find.PrecioPublico * find.Quantity;
        }
        else
        {
            obj.TransactionMasterDetailID = Helper.GetTimestampId();
            obj.Quantity                  = decimal.One;
            obj.Importe                   = obj.PrecioPublico;
            cesta.Add(obj);
        }

        RefrescarResumen();
    }

    private void OnQuitarProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        var cesta = VariablesGlobales.DtoInventario.Items;
        var find  = cesta.FirstOrDefault(response => response.ItemNumber == obj.ItemNumber);
        if (find is null) return;

        if (find.Quantity > decimal.One)
        {
            find.Quantity -= decimal.One;
            find.Importe   = find.PrecioPublico * find.Quantity;
        }
        else
        {
            cesta.Remove(find);
        }

        RefrescarResumen();
    }

    private async void OnRevisarProductos()
    {
        if (VariablesGlobales.DtoInventario.Items.Count <= 0)
        {
            ShowToast("Debe seleccionar al menos un producto", ToastDuration.Long, 12);
            return;
        }

        try
        {
            IsBusy = true;
            await NavegarARevisarAsync();
        }
        catch (Exception ex)
        {
            ShowToast($"Error al navegar: {ex.Message}", ToastDuration.Long, 12);
        }
        finally
        {
            IsBusy = false;
        }
    }

    protected void RefrescarResumen()
    {
        var cesta = VariablesGlobales.DtoInventario.Items;
        VariablesGlobales.DtoInventario.CantidadTotalSeleccionada = (int)cesta.Sum(r => r.Quantity);
        VariablesGlobales.DtoInventario.Balance                   = cesta.Sum(r => r.Importe);

        if (cesta.Count > 0)
        {
            ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInventario.CantidadTotalSeleccionada} Items";
            ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInventario.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInventario.Balance:N2}";
        }
        else
        {
            ProductosSeleccionadosCantidad      = "Seleccionar Productos";
            ProductosSeleccionadosCantidadTotal = "Items";
        }
    }

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        await LoadAllProductosAsync();
        RefrescarResumen();
    }
}
