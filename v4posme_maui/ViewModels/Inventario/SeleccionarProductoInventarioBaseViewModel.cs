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
// (Entrada y Salida). Muestra TODO el listado de productos con editores en linea:
// - Entrada (Compra): cantidad, costo y precio (costo/precio precargados del item)
// - Salida: solo cantidad
// Un producto queda "seleccionado" cuando su cantidad es mayor que cero. Al avanzar se
// arman los Items del DTO con los productos que tienen cantidad > 0.
public abstract class SeleccionarProductoInventarioBaseViewModel : BaseViewModel
{
    protected readonly IRepositoryItems RepositoryItems;
    protected readonly HelperCore Helper;

    protected SeleccionarProductoInventarioBaseViewModel()
    {
        Productos                     = new();
        RepositoryItems               = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        Helper                        = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        SearchCommand                 = new Command(OnSearch);
        SearchBarCodeCommand          = new Command(OnSearchBarCode);
        ProductosSeleccionadosCommand = new Command(OnAvanzar);
        RecalcularCommand             = new Command(RefrescarResumen);
        AtrasCommand                  = new Command(OnAtras);
    }

    // Navegacion al paso de revision (implementada por cada flujo concreto).
    protected abstract Task NavegarARevisarAsync();

    // Indica si el flujo permite editar costo y precio (Entrada = true, Salida = false).
    public abstract bool PermiteEditarPrecioCosto { get; }

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Productos { get; }

    public Command SearchCommand { get; }
    public Command SearchBarCodeCommand { get; }
    public Command ProductosSeleccionadosCommand { get; }
    public Command RecalcularCommand { get; }
    public Command AtrasCommand { get; }

    private string _resumen = "0 Items = C$ 0.00";
    public string ProductosSeleccionadosCantidadTotal
    {
        get => _resumen;
        set => SetProperty(ref _resumen, value);
    }

    private string _botonTexto = "Continuar";
    public string ProductosSeleccionadosCantidad
    {
        get => _botonTexto;
        set => SetProperty(ref _botonTexto, value);
    }

    private async void OnAtras()
    {
        await NavigationService.GoBackAsync();
    }

    private async void OnSearch()
    {
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

            // Productos ya capturados en el DTO (para conservar lo ingresado al buscar).
            var cesta = VariablesGlobales.DtoInventario.Items;

            foreach (var item in items)
            {
                item.Name          = item.Name?.ToLower();
                item.MonedaSimbolo = "C$";

                var enCesta = cesta.FirstOrDefault(c => c.ItemNumber == item.ItemNumber);
                if (enCesta is not null)
                {
                    // Conservar lo ya capturado.
                    item.Quantity      = enCesta.Quantity;
                    item.PrecioPublico = enCesta.PrecioPublico;
                    item.Cost          = enCesta.Cost;
                }
                else
                {
                    // Cantidad inicia en 0; costo y precio quedan precargados del item.
                    item.Quantity = decimal.Zero;
                }
                // Solo visualizacion: el importe mostrado es cantidad x costo.
                item.Importe = item.Cost * item.Quantity;
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

    // Recalcula el importe por producto y el resumen total a partir de las cantidades y
    // precios ingresados en linea.
    protected void RefrescarResumen()
    {
        decimal total = 0m;
        int cantidadItems = 0;
        foreach (var item in Productos)
        {
            if (item.Quantity < 0) item.Quantity = 0;
            // Solo visualizacion: el importe mostrado es cantidad x costo.
            item.Importe = item.Cost * item.Quantity;
            if (item.Quantity > 0)
            {
                total += item.Importe;
                cantidadItems++;
            }
        }

        ProductosSeleccionadosCantidadTotal = $"{cantidadItems} Items = C$ {total:N2}";
        ProductosSeleccionadosCantidad      = cantidadItems > 0 ? $"Continuar ({cantidadItems})" : "Continuar";
    }

    private async void OnAvanzar()
    {
        RefrescarResumen();

        var seleccionados = Productos.Where(p => p.Quantity > 0).ToList();
        if (seleccionados.Count == 0)
        {
            ShowToast("Debe ingresar cantidad en al menos un producto", ToastDuration.Long, 12);
            return;
        }

        // Armar la cesta del DTO con los productos capturados.
        var cesta = VariablesGlobales.DtoInventario.Items;
        cesta.Clear();
        foreach (var item in seleccionados)
        {
            item.TransactionMasterDetailID = Helper.GetTimestampId();
            // Solo visualizacion: el importe mostrado es cantidad x costo.
            item.Importe                   = item.Cost * item.Quantity;
            cesta.Add(item);
        }

        VariablesGlobales.DtoInventario.CantidadTotalSeleccionada = (int)cesta.Sum(r => r.Quantity);
        VariablesGlobales.DtoInventario.Balance                   = cesta.Sum(r => r.Importe);

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

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        await LoadAllProductosAsync();
        RefrescarResumen();
    }
}
