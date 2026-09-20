﻿using System.Collections.ObjectModel;
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
    private readonly HelperInvoiceFlow _helperInvoiceFlow;

    public SeleccionarProductoViewModel()
    {
        Title                         = "Seleccionar producto 4/6";
        Productos                     = new();
        _repositoryItems              = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _helperInvoiceFlow            = VariablesGlobales.UnityContainer.Resolve<HelperInvoiceFlow>();
        AnadirProducto                = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnAnadirProducto);
        _helper                       = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        SearchBarCodeCommand          = new Command(OnSearchBarCode);
        SearchCommand                 = new Command(OnSearch);
        ProductosSeleccionadosCommand = new Command(OnRevisarProductos);
        QuitarProductoCommand         = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnQuitarProducto);
        IrSeleccionClienteCommand     = new Command(OnIrSeleccionCliente);
        IrDatosFacturaCommand         = new Command(OnIrDatosFactura);
        IrDatosCreditoCommand         = new Command(OnIrDatosCredito);
        AbrirMenuPrincipalCommand     = new Command(OnAbrirMenuPrincipal);
        NuevaFacturaCommand           = new Command(OnNuevaFactura);
        DetalleProductoCommand        = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnDetalleProducto);
        ConfirmarDetalleCommand       = new Command(OnConfirmarDetalle);
        CancelarDetalleCommand        = new Command(OnCancelarDetalle);
    }

    // Opcion "Nueva factura" del menu desplegable (toolbar). Limpia el DtoInvoice y lo
    // reinicia con los valores por defecto para empezar una factura desde cero, sin
    // arrastrar cliente, productos, moneda, credito ni comentarios de la factura anterior.
    public Command NuevaFacturaCommand { get; }

    private async void OnNuevaFactura()
    {
        try
        {
            IsBusy = true;

            // Se reinicia el flag para forzar que InicializarFacturaRapidaAsync vuelva a
            // construir un DtoInvoice limpio con los valores por defecto.
            HelperInvoiceFlow.ReiniciarFlujo();

            // Se limpia el estado actual (productos y contadores) por si la vista mantiene
            // referencias al DTO anterior mientras se reconstruye.
            VariablesGlobales.DtoInvoice.ClearItems();
            VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada = 0;
            VariablesGlobales.DtoInvoice.Balance                   = decimal.Zero;

            await _helperInvoiceFlow.InicializarFacturaRapidaAsync();

            // Se recarga la lista de productos (sin filtro) y se refresca el resumen para
            // que la pantalla quede como una factura nueva.
            Search         = string.Empty;
            IsPanelVisible = false;
            await LoadAllProductosAsync();
            RefrescarResumenSeleccionados();

            ShowToast("Nueva factura iniciada", ToastDuration.Short, 12);
        }
        catch (Exception ex)
        {
            ShowToast($"Error al iniciar nueva factura: {ex.Message}", ToastDuration.Long, 12);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // El icono superior izquierdo (drawer) abre el menu principal (flyout) del Shell.
    // Reemplaza el boton atras para que el usuario siempre tenga acceso visible al menu
    // sin poder regresar a la pantalla anterior del flujo de facturacion.
    public Command AbrirMenuPrincipalCommand { get; }

    private void OnAbrirMenuPrincipal()
    {
        if (Shell.Current is not null)
        {
            Shell.Current.FlyoutIsPresented = true;
        }
    }

    // Navegacion desde el menu desplegable (toolbar) de la pantalla 4/6 para modificar
    // los datos de las pantallas anteriores. Los productos seleccionados se conservan
    // porque viven en VariablesGlobales.DtoInvoice.Items.
    public Command IrSeleccionClienteCommand { get; }
    public Command IrDatosFacturaCommand { get; }
    public Command IrDatosCreditoCommand { get; }

    private async void OnIrSeleccionCliente()
    {
        // Se marca que la lista de clientes se abre desde el menu desplegable para que
        // muestre la lista (y no salte automaticamente a la seleccion de producto).
        VariablesGlobales.InvoiceSeleccionandoCliente = true;
        await NavigationService.NavigateToAsync<InvoicesViewModel>();
    }

    private async void OnIrDatosFactura()
    {
        var customerNumber = VariablesGlobales.DtoInvoice.CustomerResponse?.CustomerNumber
                             ?? VariablesGlobales.DtoInvoice.CustomerNumber;
        await NavigationService.NavigateToAsync<DataInvoicesViewModel>(customerNumber!);
    }

    private async void OnIrDatosCredito()
    {
        var customerNumber = VariablesGlobales.DtoInvoice.CustomerResponse?.CustomerNumber
                             ?? VariablesGlobales.DtoInvoice.CustomerNumber;
        await NavigationService.NavigateToAsync<DataInvoiceCreditViewModel>(customerNumber!);
    }

    private async void OnRevisarProductos(object obj)
    {
        if (VariablesGlobales.DtoInvoice.Items.Count <= 0)
        {
            ShowToast(Mensajes.MensajeSeleccionarProductos, ToastDuration.Long, 12);
            return;
        }
        try
        {
            IsBusy = true;
            await NavigationService.NavigateToAsync<RevisarProductosSeleccionadosViewModel>();
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

    private async void OnSearch()
    {
        // La lupa y el Enter del teclado ejecutan la busqueda directamente,
        // sin abrir el popup. Si el popup estaba abierto se cierra.
        IsPanelVisible = false;

        if (string.IsNullOrWhiteSpace(Search))
        {
            return;
        }

        await LoadAllProductosAsync();
    }

    private async Task LoadAllProductosAsync()
    {
        IsBusy = true;
        try
        {
            List<Api_AppMobileApi_GetDataDownloadItemsResponse> items;

            if (string.IsNullOrWhiteSpace(Search))
            {
                items = await _repositoryItems.PosMeFindAll();
            }
            else
            {
                items = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndName(Search);
            }

            items = items.OrderBy(i => i.Name).ToList();

            // El simbolo de moneda puede no estar disponible si el DTO aun no fue
            // inicializado (por ejemplo justo despues de reiniciar el flujo). Se usa un
            // valor seguro para evitar NullReferenceException en el foreach.
            var monedaSimbolo = VariablesGlobales.DtoInvoice.Currency?.Simbolo ?? string.Empty;

            foreach (var item in items)
            {
                item.Name          = item.Name?.ToLower();
                item.MonedaSimbolo = monedaSimbolo;
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

    private async void OnSearchBarCode()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar         = await barCodePage.WaitForResultAsync();
        Search          = bar!;
        IsPanelVisible  = false;

        if (!string.IsNullOrWhiteSpace(Search))
        {
            await LoadAllProductosAsync();
        }
    }

    private async void OnAnadirProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        var permitirRepetidos         = await _helper.GetValueParameter("MOBILE_ALLOW_REPEATED_PRODUCTS", "false");
        var cestaArticulos            = VariablesGlobales.DtoInvoice.Items;
        var transactionMasterDetailID = _helper.GetTimestampId();

        if (permitirRepetidos == "true")
        {
            var nuevo = new Api_AppMobileApi_GetDataDownloadItemsResponse
            {
                TransactionMasterDetailID = transactionMasterDetailID,
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
                obj.TransactionMasterDetailID = transactionMasterDetailID;
                obj.Quantity                  = decimal.One;
                obj.Importe                   = obj.PrecioPublico;
                obj.MontoDescuento            = 0m;
                cestaArticulos.Add(obj);
            }
        }

        VariablesGlobales.DtoInvoice.Balance = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);
        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada++;
        ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
        ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";

        // Confirmacion visual llamativa (popup verde inferior) de que el producto se agrego.
        MostrarProductoAgregado(obj.Name ?? obj.ItemNumber ?? "Producto");
    }

    private void OnQuitarProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        var cestaArticulos = VariablesGlobales.DtoInvoice.Items;
        var find           = cestaArticulos.FirstOrDefault(response => response.ItemNumber == obj.ItemNumber);
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

        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada = cestaArticulos.Count;
        VariablesGlobales.DtoInvoice.Balance = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);
        ProductosSeleccionadosCantidad = cestaArticulos.Count > 0
            ? $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items"
            : "Seleccionar Productos";
        ProductosSeleccionadosCantidadTotal = cestaArticulos.Count > 0
            ? $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}"
            : "Items";
    }

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;

        // Facturacion rapida: si se llega a esta pantalla con el flujo sin inicializar
        // (por ejemplo, tras dar "Nueva factura" o desde el boton Facturar de la barra
        // inferior), se cargan los datos iniciales por defecto de la factura.
        await _helperInvoiceFlow.InicializarFacturaRapidaAsync();

        await LoadAllProductosAsync();

        // Al regresar desde la pantalla 6/6 los productos pueden haberse modificado
        // (cambios de cantidad, precio, descuento o eliminaciones). Se recalcula el
        // contador y el balance desde Items (fuente de verdad) para que los labels
        // reflejen el estado real y no un valor desincronizado.
        RefrescarResumenSeleccionados();
    }

    private void RefrescarResumenSeleccionados()
    {
        var cestaArticulos = VariablesGlobales.DtoInvoice.Items;

        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada = (int)cestaArticulos.Sum(r => r.Quantity);
        VariablesGlobales.DtoInvoice.Balance = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);

        if (cestaArticulos.Count > 0)
        {
            ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
            ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";
        }
        else
        {
            ProductosSeleccionadosCantidad      = "Seleccionar Productos";
            ProductosSeleccionadosCantidadTotal = "Items";
        }
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
    public Command<Api_AppMobileApi_GetDataDownloadItemsResponse> QuitarProductoCommand { get; }
    public Command<Api_AppMobileApi_GetDataDownloadItemsResponse> DetalleProductoCommand { get; }
    public Command ConfirmarDetalleCommand { get; }
    public Command CancelarDetalleCommand { get; }

    private bool _isPanelVisible;
    public bool IsPanelVisible
    {
        get => _isPanelVisible;
        set => SetProperty(ref _isPanelVisible, value);
    }

    public Command ProductosSeleccionadosCommand { get; }

    // ---- Popup de detalle: permite indicar cantidad y precio antes de agregar el producto ----

    // Producto sobre el que se abrio el popup de detalle.
    private Api_AppMobileApi_GetDataDownloadItemsResponse? _productoDetalle;

    private bool _detalleVisible;
    public bool DetalleVisible
    {
        get => _detalleVisible;
        set => SetProperty(ref _detalleVisible, value);
    }

    private string _detalleNombre = string.Empty;
    public string DetalleNombre
    {
        get => _detalleNombre;
        set => SetProperty(ref _detalleNombre, value);
    }

    private decimal _detalleCantidad = decimal.One;
    public decimal DetalleCantidad
    {
        get => _detalleCantidad;
        set => SetProperty(ref _detalleCantidad, value);
    }

    private decimal _detallePrecio;
    public decimal DetallePrecio
    {
        get => _detallePrecio;
        set => SetProperty(ref _detallePrecio, value);
    }

    // Abre el popup de detalle con la cantidad en 1 y el precio publico del producto.
    private void OnDetalleProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        _productoDetalle = obj;
        DetalleNombre    = obj.Name ?? obj.ItemNumber ?? "Producto";
        DetalleCantidad  = decimal.One;
        DetallePrecio    = obj.PrecioPublico;
        DetalleVisible   = true;
    }

    private void OnCancelarDetalle()
    {
        DetalleVisible   = false;
        _productoDetalle = null;
    }

    // Agrega el producto a la factura usando la cantidad y el precio indicados en el popup.
    // Respeta el parametro MOBILE_ALLOW_REPEATED_PRODUCTS: si es "true" siempre crea una
    // linea nueva; si es "false" acumula sobre la linea existente del mismo producto.
    private async void OnConfirmarDetalle()
    {
        if (_productoDetalle is null) return;

        if (DetalleCantidad <= decimal.Zero)
        {
            ShowToast("La cantidad debe ser mayor a cero", ToastDuration.Short, 12);
            return;
        }

        if (DetallePrecio < decimal.Zero)
        {
            ShowToast("El precio no puede ser negativo", ToastDuration.Short, 12);
            return;
        }

        var permitirRepetidos         = await _helper.GetValueParameter("MOBILE_ALLOW_REPEATED_PRODUCTS", "false");
        var cestaArticulos            = VariablesGlobales.DtoInvoice.Items;
        var transactionMasterDetailID = _helper.GetTimestampId();

        if (permitirRepetidos == "true")
        {
            // Siempre se crea una linea nueva con la cantidad y precio indicados.
            var nuevo = new Api_AppMobileApi_GetDataDownloadItemsResponse
            {
                TransactionMasterDetailID = transactionMasterDetailID,
                ItemPk              = _productoDetalle.ItemPk,
                ItemId              = _productoDetalle.ItemId,
                BarCode             = _productoDetalle.BarCode,
                ItemNumber          = _productoDetalle.ItemNumber,
                Name                = _productoDetalle.Name,
                PrecioPublico       = DetallePrecio,
                CantidadEntradas    = _productoDetalle.CantidadEntradas,
                CantidadSalidas     = _productoDetalle.CantidadSalidas,
                CantidadFinal       = _productoDetalle.CantidadFinal,
                MonedaSimbolo       = _productoDetalle.MonedaSimbolo,
                Quantity            = DetalleCantidad,
                MontoDescuento      = 0m,
                PorcentajeDescuento = 0m,
                Importe             = DetallePrecio * DetalleCantidad
            };
            cestaArticulos.Add(nuevo);
        }
        else
        {
            // Si ya existe el producto, se acumula la cantidad y se actualiza el precio y el
            // importe. Si no existe, se agrega como linea nueva.
            var find = cestaArticulos.FirstOrDefault(response => response.ItemNumber == _productoDetalle.ItemNumber);
            if (find is not null)
            {
                find.Quantity      += DetalleCantidad;
                find.PrecioPublico = DetallePrecio;
                find.Importe       = find.PrecioPublico * find.Quantity;
                find.MontoDescuento = find.PorcentajeDescuento > 0
                    ? find.Importe * (find.PorcentajeDescuento / 100m)
                    : find.MontoDescuento;
            }
            else
            {
                var nuevo = new Api_AppMobileApi_GetDataDownloadItemsResponse
                {
                    TransactionMasterDetailID = transactionMasterDetailID,
                    ItemPk              = _productoDetalle.ItemPk,
                    ItemId              = _productoDetalle.ItemId,
                    BarCode             = _productoDetalle.BarCode,
                    ItemNumber          = _productoDetalle.ItemNumber,
                    Name                = _productoDetalle.Name,
                    PrecioPublico       = DetallePrecio,
                    CantidadEntradas    = _productoDetalle.CantidadEntradas,
                    CantidadSalidas     = _productoDetalle.CantidadSalidas,
                    CantidadFinal       = _productoDetalle.CantidadFinal,
                    MonedaSimbolo       = _productoDetalle.MonedaSimbolo,
                    Quantity            = DetalleCantidad,
                    MontoDescuento      = 0m,
                    PorcentajeDescuento = 0m,
                    Importe             = DetallePrecio * DetalleCantidad
                };
                cestaArticulos.Add(nuevo);
            }
        }

        VariablesGlobales.DtoInvoice.Balance = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);
        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada = (int)cestaArticulos.Sum(r => r.Quantity);
        ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
        ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";

        MostrarProductoAgregado(_productoDetalle.Name ?? _productoDetalle.ItemNumber ?? "Producto");

        DetalleVisible   = false;
        _productoDetalle = null;
    }

    // Popup verde inferior que confirma visualmente que se agrego un producto a la factura.
    private bool _productoAgregadoVisible;
    public bool ProductoAgregadoVisible
    {
        get => _productoAgregadoVisible;
        set => SetProperty(ref _productoAgregadoVisible, value);
    }

    private string _productoAgregadoMensaje = string.Empty;
    public string ProductoAgregadoMensaje
    {
        get => _productoAgregadoMensaje;
        set => SetProperty(ref _productoAgregadoMensaje, value);
    }

    private string _productoAgregadoDetalle = string.Empty;
    public string ProductoAgregadoDetalle
    {
        get => _productoAgregadoDetalle;
        set => SetProperty(ref _productoAgregadoDetalle, value);
    }

    // Controla cada "aparicion" del popup para poder auto-ocultarlo sin que un producto
    // agregado antes cierre el popup de uno agregado despues (evita cierres prematuros).
    private int _productoAgregadoToken;

    // Muestra el popup verde con el nombre del producto agregado y el total actual, y lo
    // oculta automaticamente despues de unos segundos.
    private void MostrarProductoAgregado(string nombreProducto)
    {
        var simbolo = VariablesGlobales.DtoInvoice.Currency?.Simbolo ?? string.Empty;

        ProductoAgregadoMensaje = $"✓ {nombreProducto?.ToLower()} agregado";
        ProductoAgregadoDetalle = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} items · {simbolo} {VariablesGlobales.DtoInvoice.Balance:N2}";
        ProductoAgregadoVisible = true;

        var token = ++_productoAgregadoToken;
        _ = OcultarProductoAgregadoAsync(token);
    }

    private async Task OcultarProductoAgregadoAsync(int token)
    {
        await Task.Delay(2000);
        // Solo se oculta si no hubo un nuevo producto agregado mientras tanto.
        if (token == _productoAgregadoToken)
        {
            ProductoAgregadoVisible = false;
        }
    }
}
