using System.Collections.ObjectModel;
using System.Diagnostics;
using Android.Speech;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core.Internal;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;
using v4posme_maui.Views.Abonos;
using v4posme_maui.Views.Invoices;
using v4posme_maui.Views.Printers;
using v4posme_maui.Views.More.Gasto;
using v4posme_maui.Views.Inventario;
using Unity;
using v4posme_maui.Services.Helpers;
using Android.Test.Suitebuilder.Annotation;

namespace v4posme_maui.ViewModels.Printers;

public class DashboardPrinterViewModel : BaseViewModel
{
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail;
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;
    private readonly IRepositoryTbCustomer _repositoryTbCustomer;
    private readonly IRepositoryItems _repositoryItems;
    private const int UnselectedIndex = -1;
    private readonly HelperCore _helper;
    public DashboardPrinterViewModel()
    {
        _repositoryTbTransactionMaster          = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryTbTransactionMasterDetail    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        _repositoryDocumentCredit               = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        _repositoryItems                        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _repositoryTbCustomer                   = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _helper                                 = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        Facturas                                = new();
        Abonos                                  = new();
        Productos                               = new();
        Gastos                                  = new();
        InventarioEntradas                      = new();
        InventarioSalidas                       = new();
        SearchInventarioEntradasCommand         = new Command(OnSearchInventarioEntradasCommand);
        SearchInventarioSalidasCommand          = new Command(OnSearchInventarioSalidasCommand);
        SelectedInventarioEntradaCommand        = new Command<ViewTempDtoInventarioLista>(OnSelectedInventarioEntradaCommand);
        SelectedInventarioSalidaCommand         = new Command<ViewTempDtoInventarioLista>(OnSelectedInventarioSalidaCommand);
        OnBarCode                               = new Command(OnSearchBarCode);
        SearchFacturaCommand                    = new Command(OnSearchFacturaCommand);
        SelectedFacturaCommand                  = new Command<ViewTempDtoInvoice>(OnSelectedFacturaCommand);
        SelectedAbonoCommand                    = new Command<ViewTempDtoAbono>(OnSelectedAbonoCommand);
        SelectedProductoCommand                 = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnSelectedProductoCommand);
        SelectedGastoCommand                    = new Command<ViewTempDtoGastoLista>(OnSelectedGastoCommand);
        SearchAbonoCommand                      = new Command(OnSearchAbonoCommand);
        SearchProductCommand                    = new Command(OnSearchProductCommand);
        SearchGastoCommand                      = new Command(OnSearchGastoCommand);
        IsBusy                                  = true;
    }

    private async void OnSearchGastoCommand()
    {
        IsBusy = true;
        List<TbTransactionMaster> filters;
        if (string.IsNullOrWhiteSpace(SearchGastos))
        {
            filters = await _repositoryTbTransactionMaster.PosMeFilterTop10Gastos();
        }
        else
        {
            filters = await _repositoryTbTransactionMaster.PosMeFilterTop10ByCodigoGastos(SearchGastos);
        }

        await FillGastos(filters);
        IsBusy = false;
    }

    private async void OnSelectedGastoCommand(ViewTempDtoGastoLista obj)
    {
        try
        {
            VariablesGlobales.DtoGasto = new ViewTempDtoGasto
            {
                TransactionMasterId = obj.TransactionMasterId,
                NumeroGasto   = obj.Codigo,
                Fecha         = obj.Fecha,
                MonedaNombre  = obj.MonedaNombre,
                MonedaSimbolo = obj.MonedaSimbolo,
                Monto         = obj.Monto,
                Comentario    = obj.Comentario,
                Referencia1   = obj.Referencia1,
                Referencia2   = obj.Referencia2
            };

            await Navigation!.PushAsync(new GastoComprobantePage());
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }

    private async void OnSearchProductCommand()
    {
        IsBusy = true;
        List<Api_AppMobileApi_GetDataDownloadItemsResponse> searchItems;
        if (string.IsNullOrWhiteSpace(SearchProduct))
        {
            searchItems = await _repositoryItems.PosMeOrderByNameLowerTake10();
        }
        else
        {
            searchItems = await _repositoryItems.PosMeFilterByItemNumberAndBarCodeAndNameOrderByNameTake10(SearchProduct);
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Productos = new ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>(searchItems);
        });
        IsBusy = false;
    }

    private async void OnSelectedProductoCommand(Api_AppMobileApi_GetDataDownloadItemsResponse obj)
    {
        VariablesGlobales.Item = obj;
        await Navigation!.PushAsync(new PrinterProductoPage());
    }

    private async void OnSelectedAbonoCommand(ViewTempDtoAbono obj)
    {
        try
        {
            VariablesGlobales.DtoAplicarAbono   = obj;
            var mostrarPrintSinSaldos           = await _helper.GetValueParameter("CXC_SHOW_BALANCE_IN_SHARE_MOBILE","false");
            var typePrinterShare                = await _helper.GetValueParameter("CXC_TYPE_PRINTER_SHARE_MOBILE","DEFAULT");
        
            if (typePrinterShare=="FINANCIAL")
            {
                await Navigation!.PushAsync(new ValidarAbonoFinancieraPage(), true);
            }
            else if (mostrarPrintSinSaldos == "true")
            {
                await Navigation!.PushAsync(new PrinterAbonoPage(), true);
            }
            else
            {
                await Navigation!.PushAsync(new ValidarAbonoHideSaldoPage(), true);
            }
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }

    private async void OnSearchAbonoCommand(object obj)
    {
        IsBusy = true;
        List<TbTransactionMaster> filters;
        if (string.IsNullOrWhiteSpace(SearchAbonos))
        {
            filters = await _repositoryTbTransactionMaster.PosMeFilterTop10Abonos();
        }
        else
        {
            filters = await _repositoryTbTransactionMaster.PosMeFilterTop10ByCodigoAndNombreClienteAbonos(SearchAbonos);
        }

        await FillAbonos(filters);
        IsBusy = false;
    }

    private async void OnSelectedFacturaCommand(ViewTempDtoInvoice obj)
    {
        try
        {
            VariablesGlobales.DtoInvoice                    = obj;
            var transactionMasterId                         = VariablesGlobales.DtoInvoice.TransactionMasterId;
            var transactionMasterDetailItems                = await _repositoryTbTransactionMasterDetail.PosMeItemByTransactionId(transactionMasterId);
            VariablesGlobales.DtoInvoice.TransactionMaster  = await _repositoryTbTransactionMaster.PosMeFindByTransactionId(transactionMasterId);

            VariablesGlobales.DtoInvoice.TransactionMasterId= transactionMasterId;
            VariablesGlobales.DtoInvoice.Monto              = VariablesGlobales.DtoInvoice.TransactionMaster.Amount;
            VariablesGlobales.DtoInvoice.TipoPayment        = VariablesGlobales.DtoInvoice.TransactionMaster.TypePaymentId;
            VariablesGlobales.DtoInvoice.Comentarios        = VariablesGlobales.DtoInvoice.TransactionMaster.Comment;
            VariablesGlobales.DtoInvoice.CustomerResponse                       = await _repositoryTbCustomer.PosMeFindEntityId(VariablesGlobales.DtoInvoice.TransactionMaster.EntityId);
            VariablesGlobales.DtoInvoice.CustomerResponse.CustomerCreditLineId  = VariablesGlobales.DtoInvoice.TransactionMaster.CustomerCreditLineId;
            VariablesGlobales.DtoInvoice.CustomerResponse.Identification        = VariablesGlobales.DtoInvoice.TransactionMaster.CustomerIdentification;
            
            VariablesGlobales.DtoInvoice.Codigo                 = VariablesGlobales.DtoInvoice.TransactionMaster.TransactionNumber!;
            VariablesGlobales.DtoInvoice.Plazo                  = VariablesGlobales.DtoInvoice.TransactionMaster.Plazo;
            VariablesGlobales.DtoInvoice.NextVisit              = VariablesGlobales.DtoInvoice.TransactionMaster.NextVisit;
            VariablesGlobales.DtoInvoice.FixedExpenses          = VariablesGlobales.DtoInvoice.TransactionMaster.FixedExpenses;
            VariablesGlobales.DtoInvoice.ReferenceClientName    = VariablesGlobales.DtoInvoice.TransactionMaster.ReferenceClientName;
            VariablesGlobales.DtoInvoice.Cambio                 = 0;
            VariablesGlobales.DtoInvoice.TransactionOn          = DateTime.Now;


            VariablesGlobales.DtoInvoice.Currency           = new DtoCatalogItem((int)VariablesGlobales.DtoInvoice.TransactionMaster.CurrencyId, VariablesGlobales.DtoInvoice.TransactionMaster.CurrencyId.ToString(), VariablesGlobales.DtoInvoice.TransactionMaster.CurrencyId.ToString());
            VariablesGlobales.DtoInvoice.TipoDocumento      = new DtoCatalogItem((int)VariablesGlobales.DtoInvoice.TransactionMaster.TransactionCausalId, VariablesGlobales.DtoInvoice.TransactionMaster.TransactionCausalId.ToString(), VariablesGlobales.DtoInvoice.TransactionMaster.TransactionCausalId.ToString());
            VariablesGlobales.DtoInvoice.PeriodPay          = new DtoCatalogItem((int)VariablesGlobales.DtoInvoice.TransactionMaster.PeriodPay, VariablesGlobales.DtoInvoice.TransactionMaster.PeriodPay.ToString(), VariablesGlobales.DtoInvoice.TransactionMaster.PeriodPay.ToString());
            VariablesGlobales.DtoInvoice.Mesa               = new DtoCatalogItem((int)VariablesGlobales.DtoInvoice.TransactionMaster.MesaID, VariablesGlobales.DtoInvoice.TransactionMaster.MesaID.ToString(), VariablesGlobales.DtoInvoice.TransactionMaster.MesaID.ToString());
            VariablesGlobales.DtoInvoice.Balance            = VariablesGlobales.DtoInvoice.TransactionMaster.SubAmount - VariablesGlobales.DtoInvoice.TransactionMaster.Discount;
            VariablesGlobales.DtoInvoice.ClearItems();

            foreach(var items in transactionMasterDetailItems)
            {
                Api_AppMobileApi_GetDataDownloadItemsResponse item  = await _repositoryItems.PosMeFindByItemId(items.ComponentItemId);
                item.Quantity                                       = items.Quantity;
                item.Importe                                        = items.SubAmount;
                item.MontoDescuento                                 = items.Discount;                
                item.PrecioPublico                                  = items.UnitaryPrice;
                item.Referencia                                     = items.ReferenciaProducto;
                item.TransactionMasterDetailID                      = _helper.GetTimestampId();
                VariablesGlobales.DtoInvoice.Items.Add(item);
            }
            
            VariablesGlobales.DtoInvoice.Balance                    = VariablesGlobales.DtoInvoice.Items.Sum(response => response.Importe) - VariablesGlobales.DtoInvoice.Items.Sum(response => response.MontoDescuento);
            VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada  = VariablesGlobales.DtoInvoice.Items.Count();

            VariablesGlobales.EnableBackButton              = true;
            await Navigation!.PushAsync(new VoucherInvoicePage());
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 12);
        }
    }

    private async void OnSearchFacturaCommand()
    {
        IsBusy = true;
        List<TbTransactionMaster> findAllFactura;
        if (string.IsNullOrWhiteSpace(Search))
        {
            findAllFactura = await _repositoryTbTransactionMaster.PosMeFilterTop10Facturas();
        }
        else
        {
            findAllFactura = await _repositoryTbTransactionMaster.PosMeFilterTop10ByCodigoAndNombreClienteFacturas(Search);
        }

        await FillFacturas(findAllFactura);
        IsBusy = false;
    }

    private async void OnSearchBarCode()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var barCode = await barCodePage.WaitForResultAsync();
        switch (SelectedIndex)
        {
            case 0:
                Search = barCode!;
                break;
            case 2:
                SearchProduct = barCode!;
                break;
        }
    }

    public DXObservableCollection<ViewTempDtoInvoice> Facturas { get; }

    private ObservableCollection<ViewTempDtoAbono>? _abonos;

    public ObservableCollection<ViewTempDtoAbono> Abonos
    {
        get => _abonos!;
        set => SetProperty(ref _abonos, value);
    }

    private ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>? _productos;

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Productos
    {
        get => _productos!;
        set => SetProperty(ref _productos, value);
    }

    private ObservableCollection<ViewTempDtoGastoLista>? _gastos;

    public ObservableCollection<ViewTempDtoGastoLista> Gastos
    {
        get => _gastos!;
        set => SetProperty(ref _gastos, value);
    }

    private string _searchGastos;

    public string SearchGastos
    {
        get => _searchGastos;
        set => SetProperty(ref _searchGastos, value);
    }

    public Command SearchGastoCommand { get; }
    public Command SelectedGastoCommand { get; }

    public Command SearchFacturaCommand { get; }
    public Command OnBarCode { get; }

    private int _selectedIndex;

    public int SelectedIndex
    {
        get => _selectedIndex;
        set
        {
            SetProperty(ref _selectedIndex, value, nameof(SelectedIndex), ()=> _ = Load(value));
        }
    }

    public Command<ViewTempDtoInvoice> SelectedFacturaCommand { get; }
    private string _totalCordobasFacturado;

    public string TotalCordobasFacturado
    {
        get => _totalCordobasFacturado;
        set => SetProperty(ref _totalCordobasFacturado, value);
    }

    private string _totalDolaresFacturado;

    public string TotalDolaresFacturado
    {
        get => _totalDolaresFacturado;
        set => SetProperty(ref _totalDolaresFacturado, value);
    }

    private string _searchAbonos;

    public string SearchAbonos
    {
        get => _searchAbonos;
        set => SetProperty(ref _searchAbonos, value);
    }

    public Command SearchAbonoCommand { get; }

    private string _totalCordobasAbonos;

    public string TotalCordobasAbonos
    {
        get => _totalCordobasAbonos;
        set => SetProperty(ref _totalCordobasAbonos, value);
    }

    private string _totalDolaresAbonos;

    public string TotalDolaresAbonos
    {
        get => _totalDolaresAbonos;
        set => SetProperty(ref _totalDolaresAbonos, value);
    }

    public Command SelectedAbonoCommand { get; }
    public Command SelectedProductoCommand { get; }
    private string _searchProduct;

    public string SearchProduct
    {
        get => _searchProduct;
        set => SetProperty(ref _searchProduct, value);
    }

    public Command SearchProductCommand { get; }

    private ObservableCollection<ViewTempDtoInventarioLista>? _inventarioEntradas;
    public ObservableCollection<ViewTempDtoInventarioLista> InventarioEntradas
    {
        get => _inventarioEntradas!;
        set => SetProperty(ref _inventarioEntradas, value);
    }

    private ObservableCollection<ViewTempDtoInventarioLista>? _inventarioSalidas;
    public ObservableCollection<ViewTempDtoInventarioLista> InventarioSalidas
    {
        get => _inventarioSalidas!;
        set => SetProperty(ref _inventarioSalidas, value);
    }

    private string _searchInventarioEntradas = string.Empty;
    public string SearchInventarioEntradas
    {
        get => _searchInventarioEntradas;
        set => SetProperty(ref _searchInventarioEntradas, value);
    }

    private string _searchInventarioSalidas = string.Empty;
    public string SearchInventarioSalidas
    {
        get => _searchInventarioSalidas;
        set => SetProperty(ref _searchInventarioSalidas, value);
    }

    public Command SearchInventarioEntradasCommand { get; }
    public Command SearchInventarioSalidasCommand { get; }
    public Command SelectedInventarioEntradaCommand { get; }
    public Command SelectedInventarioSalidaCommand { get; }

    private async void OnSearchInventarioEntradasCommand()
    {
        IsBusy = true;
        var filters = string.IsNullOrWhiteSpace(SearchInventarioEntradas)
            ? await _repositoryTbTransactionMaster.PosMeFilterInventarioByTransactionId((int)TypeTransaction.TransactionInventarioEntrada)
            : await _repositoryTbTransactionMaster.PosMeFilterInventarioByCodigo((int)TypeTransaction.TransactionInventarioEntrada, SearchInventarioEntradas);
        await FillInventario(filters, esEntrada: true);
        IsBusy = false;
    }

    private async void OnSearchInventarioSalidasCommand()
    {
        IsBusy = true;
        var filters = string.IsNullOrWhiteSpace(SearchInventarioSalidas)
            ? await _repositoryTbTransactionMaster.PosMeFilterInventarioByTransactionId((int)TypeTransaction.TransactionInventarioSalida)
            : await _repositoryTbTransactionMaster.PosMeFilterInventarioByCodigo((int)TypeTransaction.TransactionInventarioSalida, SearchInventarioSalidas);
        await FillInventario(filters, esEntrada: false);
        IsBusy = false;
    }

    private async Task FillInventario(List<TbTransactionMaster> masters, bool esEntrada)
    {
        var buffer = new List<ViewTempDtoInventarioLista>(masters.Count);
        foreach (var master in masters)
        {
            var detalles = await _repositoryTbTransactionMasterDetail.PosMeItemByTransactionId(master.TransactionMasterId);
            buffer.Add(new ViewTempDtoInventarioLista
            {
                TransactionMasterId = master.TransactionMasterId,
                Codigo              = master.TransactionNumber!,
                Fecha               = master.TransactionOn,
                CantidadProductos   = detalles.Count,
                Comentario          = master.Comment ?? string.Empty,
                Referencia1         = master.Reference1 ?? string.Empty,
                Referencia2         = master.Reference2 ?? string.Empty,
                CostoTotal          = master.Amount,
                MonedaSimbolo       = "C$"
            });
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            if (esEntrada)
                InventarioEntradas = new ObservableCollection<ViewTempDtoInventarioLista>(buffer);
            else
                InventarioSalidas = new ObservableCollection<ViewTempDtoInventarioLista>(buffer);
        });
    }

    private async void OnSelectedInventarioEntradaCommand(ViewTempDtoInventarioLista obj)
    {
        await AbrirInventario(obj, TypeTransaction.TransactionInventarioEntrada);
    }

    private async void OnSelectedInventarioSalidaCommand(ViewTempDtoInventarioLista obj)
    {
        await AbrirInventario(obj, TypeTransaction.TransactionInventarioSalida);
    }

    private async Task AbrirInventario(ViewTempDtoInventarioLista obj, TypeTransaction tipo)
    {
        try
        {
            var master   = await _repositoryTbTransactionMaster.PosMeFindByTransactionId(obj.TransactionMasterId);
            var detalles = await _repositoryTbTransactionMasterDetail.PosMeItemByTransactionId(obj.TransactionMasterId);

            var dto = new ViewTempDtoInventario
            {
                TransactionId           = tipo,
                TransactionMasterId     = master.TransactionMasterId,
                TransactionMaster       = master,
                Codigo                  = master.TransactionNumber!,
                Comentarios             = master.Comment,
                Referencia1             = master.Reference1,
                Referencia2             = master.Reference2,
                TransactionOn           = master.TransactionOn,
                Balance                 = master.Amount,
                AbiertoDesdeImpresiones = true
            };

            foreach (var detalle in detalles)
            {
                var item          = await _repositoryItems.PosMeFindByItemId(detalle.ComponentItemId);
                item.Quantity     = detalle.Quantity;
                item.PrecioPublico = detalle.UnitaryPrice;
                item.Cost         = detalle.UnitaryCost;
                item.Importe      = detalle.SubAmount;
                dto.Items.Add(item);
            }

            VariablesGlobales.DtoInventario = dto;

            if (tipo == TypeTransaction.TransactionInventarioEntrada)
                await Navigation!.PushAsync(new VisualizarEntradaPage());
            else
                await Navigation!.PushAsync(new VisualizarSalidaPage());
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 12);
        }
    }

    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            Navigation = navigation;
            await Load(SelectedIndex);
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }

    public async Task Load(int index)
    {
        try
        {
            IsBusy = true;

            switch (index)
            {
                case 0:
                    var findAllFactura = await _repositoryTbTransactionMaster.PosMeFilterTop10Facturas();
                    await FillFacturas(findAllFactura);
                    break;

                case 1:
                    var findAllAbonos = await _repositoryTbTransactionMaster.PosMeFilterTop10Abonos();
                    await FillAbonos(findAllAbonos);
                    break;

                case 2:
                    var findAllProductos = await _repositoryItems.PosMeNameAsc10();
                    await FillProductos(findAllProductos);
                    break;

                case 3:
                    var findAllGastos = await _repositoryTbTransactionMaster.PosMeFilterTop10Gastos();
                    await FillGastos(findAllGastos);
                    break;

                case 4:
                    var entradas = await _repositoryTbTransactionMaster.PosMeFilterInventarioByTransactionId((int)TypeTransaction.TransactionInventarioEntrada);
                    await FillInventario(entradas, esEntrada: true);
                    break;

                case 5:
                    var salidas = await _repositoryTbTransactionMaster.PosMeFilterInventarioByTransactionId((int)TypeTransaction.TransactionInventarioSalida);
                    await FillInventario(salidas, esEntrada: false);
                    break;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task FillGastos(List<TbTransactionMaster> findAllGastos)
    {
        try
        {
            var buffer = new List<ViewTempDtoGastoLista>(findAllGastos.Count);
            foreach (var master in findAllGastos)
            {
                var esCordoba = master.CurrencyId == TypeCurrency.Cordoba;
                buffer.Add(new ViewTempDtoGastoLista
                {
                    TransactionMasterId = master.TransactionMasterId,
                    Codigo              = master.TransactionNumber!,
                    Fecha               = master.TransactionOn,
                    Monto               = master.Amount,
                    MonedaSimbolo       = esCordoba ? "C$" : "$",
                    MonedaNombre        = esCordoba ? "Cordoba (C$)" : "Dolar ($)",
                    Comentario          = master.Comment ?? string.Empty,
                    Referencia1         = master.Reference1 ?? string.Empty,
                    Referencia2         = master.Reference2 ?? string.Empty
                });
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Gastos = new ObservableCollection<ViewTempDtoGastoLista>(buffer);
            });
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }

    private async Task FillProductos(List<Api_AppMobileApi_GetDataDownloadItemsResponse> findAllProductos)
    {
        try
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Productos = new ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>(findAllProductos);
            });
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }

    private async Task FillAbonos(List<TbTransactionMaster> findAllAbonos)
    {
        try
        {
            var totalCordobas = decimal.Zero;
            var totalDolares = decimal.Zero;
            var buffer = new List<ViewTempDtoAbono>(findAllAbonos.Count);

            foreach (var abono in findAllAbonos)
            {
                var customer = await _repositoryTbCustomer.PosMeFindEntityId(abono.EntityId);
                string currencyName;
                if (abono.CurrencyId == TypeCurrency.Cordoba)
                {
                    totalCordobas += abono.SubAmount;
                    currencyName = "C$";
                }
                else
                {
                    totalDolares += abono.SubAmount;
                    currencyName = "$";
                }

                var tmpAbono = new ViewTempDtoAbono(
                    abono.TransactionNumber!,
                    abono.EntityId, customer.FirstName!,
                    customer.LastName!,
                    customer.Identification!,
                    abono.TransactionOn,
                    abono.Reference4,
                    currencyName,
                    abono.SubAmount,
                    abono.Discount,
                    abono.Amount,
                    abono.Comment!);
                
                if (!string.IsNullOrWhiteSpace(abono.Reference2))
                {
                    tmpAbono.MontoMora = Convert.ToDecimal(abono.Reference2);
                }
                if (!string.IsNullOrWhiteSpace(abono.Reference3))
                {
                    tmpAbono.MoraPagada = Convert.ToDecimal(abono.Reference3);
                }
                tmpAbono.Documentos         = abono.Reference1;
                tmpAbono.DiasMora           = abono.Plazo;
                tmpAbono.CuotasPendientes   = abono.CuotasPendientes;
                buffer.Add(tmpAbono);
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                Abonos = new ObservableCollection<ViewTempDtoAbono>(buffer);
                TotalCordobasAbonos = $"C$ {totalCordobas:N2}";
                TotalDolaresAbonos = $"$ {totalDolares:N2}";
            });
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }

    private async Task FillFacturas(List<TbTransactionMaster> findAllFactura)
    {
        var totalCordobas   = decimal.Zero;
        var totalDolares    = decimal.Zero;
        var buffer          = new List<ViewTempDtoInvoice>(findAllFactura.Count);
        foreach (var master in findAllFactura)
        {
            if (master.StatusID != (int)TypeStatusBilling.Register && master.RegisterLocal != 1)
                continue;

            var dto = new ViewTempDtoInvoice
            {
                Balance             = master.SubAmount,
                TransactionOn       = master.TransactionOn,
                Codigo              = master.TransactionNumber!,
                Monto               = master.SubAmount - master.Discount,
                Cambio              = decimal.Subtract(master.Amount, master.SubAmount),
                Comentarios         = master.Comment,
                ReferenceClientName = master.ReferenceClientName,                
                TransactionMasterId = master.TransactionMasterId
                
            };
            if (master.CurrencyId == TypeCurrency.Cordoba)
            {
                dto.Currency = new DtoCatalogItem((int)master.CurrencyId, "Cordoba", "C$");
                totalCordobas += master.SubAmount - master.Discount;
            }
            else
            {
                dto.Currency = new DtoCatalogItem((int)master.CurrencyId, "Dolar", "$");
                totalDolares += master.SubAmount - master.Discount;
            }

            dto.Mesa                         = new DtoCatalogItem(master.MesaID, master.MesaName, master.MesaName);
            dto.CustomerResponse             = await _repositoryTbCustomer.PosMeFindEntityId(master.EntityId);
            dto.FirstName                    = dto.CustomerResponse.FirstName;
            dto.LastName                     = dto.CustomerResponse.LastName;
            
            var findTransactionMasterDetails = await _repositoryTbTransactionMasterDetail.PosMeItemByTransactionId(master.TransactionMasterId);
            foreach (var detail in findTransactionMasterDetails)
            {
                var findItem            = await _repositoryItems.PosMeFindByItemId(detail.ComponentItemId);
                findItem.Importe        = detail.SubAmount;
                findItem.Quantity       = detail.Quantity;
                findItem.PrecioPublico  = detail.UnitaryPrice;
                findItem.MontoDescuento = detail.Discount;
                dto.Items.Add(findItem);
            }

            buffer.Add(dto);
        }

        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            Facturas.Clear();
            Facturas.AddRange(buffer);
            TotalCordobasFacturado  = $"C$ {totalCordobas:N2}";
            TotalDolaresFacturado   = $"$ {totalDolares:N2}";
        });
    }
}