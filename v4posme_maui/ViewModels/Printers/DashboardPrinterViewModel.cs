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
using Unity;
using v4posme_maui.Services.Helpers;

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
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryTbTransactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        _repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        Facturas = new();
        Abonos = new();
        Productos = new();
        OnBarCode = new Command(OnSearchBarCode);
        SearchFacturaCommand = new Command(OnSearchFacturaCommand);
        SelectedFacturaCommand = new Command<ViewTempDtoInvoice>(OnSelectedFacturaCommand);
        SelectedAbonoCommand = new Command<ViewTempDtoAbono>(OnSelectedAbonoCommand);
        SelectedProductoCommand = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnSelectedProductoCommand);
        SearchAbonoCommand = new Command(OnSearchAbonoCommand);
        SearchProductCommand = new Command(OnSearchProductCommand);
        IsBusy = true;
    }

    private async void OnSearchProductCommand()
    {
        IsBusy = true;
        await Task.Run(async () =>
        {
            Productos.Clear();
            List<Api_AppMobileApi_GetDataDownloadItemsResponse> searchItems;
            if (string.IsNullOrWhiteSpace(SearchProduct))
            {
                searchItems = await _repositoryItems.PosMeTake10();
            }
            else
            {
                searchItems = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndName(SearchProduct);
            }

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
            VariablesGlobales.DtoAplicarAbono = obj;
            var mostrarPrintSinSaldos = await _helper.GetValueParameter("CXC_SHOW_BALANCE_IN_SHARE_MOBILE","false");
            var typePrinterShare = await _helper.GetValueParameter("CXC_TYPE_PRINTER_SHARE_MOBILE","DEFAULT");
        
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
        Abonos.Clear();
        List<TbTransactionMaster> filters;
        if (string.IsNullOrWhiteSpace(SearchAbonos))
        {
            filters = await _repositoryTbTransactionMaster.PosMeFilterAbonos();
        }
        else
        {
            filters = await _repositoryTbTransactionMaster.PosMeFilterByCodigoAndNombreClienteAbonos(SearchAbonos);
        }

        FillAbonos(filters);
        IsBusy = false;
    }

    private async void OnSelectedFacturaCommand(ViewTempDtoInvoice obj)
    {
        try
        {
            VariablesGlobales.DtoInvoice = obj;
            var transactionMasterId = VariablesGlobales.DtoInvoice.TransactionMasterId;
            VariablesGlobales.DtoInvoice.TransactionMaster = await _repositoryTbTransactionMaster.PosMeFindByTransactionId(transactionMasterId);
            VariablesGlobales.DtoInvoice.TransactionMasterId= transactionMasterId;
            VariablesGlobales.DtoInvoice.TipoDocumento = new DtoCatalogItem(
                (int)VariablesGlobales.DtoInvoice.TransactionMaster.TransactionCausalId, 
                VariablesGlobales.DtoInvoice.TransactionMaster.TransactionCausalId.ToString(), 
                VariablesGlobales.DtoInvoice.TransactionMaster.TransactionCausalId.ToString());
            VariablesGlobales.EnableBackButton = true;
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
        Facturas.Clear();
        List<TbTransactionMaster> findAllFactura;
        if (string.IsNullOrWhiteSpace(Search))
        {
            findAllFactura = await _repositoryTbTransactionMaster.PosMeFilterFacturas();
        }
        else
        {
            findAllFactura = await _repositoryTbTransactionMaster.PosMeFilterByCodigoAndNombreClienteFacturas(Search);
        }

        FillFacturas(findAllFactura);
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
                    var findAllFactura = await _repositoryTbTransactionMaster.PosMeFilterFacturas();
                    await MainThread.InvokeOnMainThreadAsync(() => 
                    {
                        FillFacturas(findAllFactura);
                    });
                    break;

                case 1:
                    var findAllAbonos = await _repositoryTbTransactionMaster.PosMeFilterAbonos();
                    await MainThread.InvokeOnMainThreadAsync(() => 
                    {
                        FillAbonos(findAllAbonos);
                    });
                    break;

                case 2:
                    var findAllProductos = await _repositoryItems.PosMeDescending10();
                    await MainThread.InvokeOnMainThreadAsync(() => 
                    {
                        Productos.Clear();
                        foreach (var item in findAllProductos)
                        {
                            Productos.Add(item);
                        }
                    });
                    break;
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void FillAbonos(List<TbTransactionMaster> findAllAbonos)
    {
        try
        {
            var totalCordobas = decimal.Zero;
            var totalDolares = decimal.Zero;
            Abonos.Clear();
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
                tmpAbono.Documentos = abono.Reference1;
                tmpAbono.DiasMora = abono.Plazo;
                tmpAbono.CuotasPendientes = abono.CuotasPendientes;
                Abonos.Add(tmpAbono);
            }

            TotalCordobasAbonos = $"C$ {totalCordobas:N2}";
            TotalDolaresAbonos = $"$ {totalDolares:N2}";
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }

    private async void FillFacturas(List<TbTransactionMaster> findAllFactura)
    {
        var totalCordobas = decimal.Zero;
        var totalDolares = decimal.Zero;
        Facturas.Clear();
        foreach (var master in findAllFactura)
        {
            var dto = new ViewTempDtoInvoice
            {
                Balance = master.SubAmount,
                TransactionOn = master.TransactionOn,
                Codigo = master.TransactionNumber!,
                Monto = master.SubAmount,
                Cambio = decimal.Subtract(master.Amount, master.SubAmount),
                Comentarios = master.Comment,
                TransactionMasterId = master.TransactionMasterId
            };
            if (master.CurrencyId == TypeCurrency.Cordoba)
            {
                dto.Currency = new DtoCatalogItem((int)master.CurrencyId, "Cordoba", "C$");
                totalCordobas += master.SubAmount;
            }
            else
            {
                dto.Currency = new DtoCatalogItem((int)master.CurrencyId, "Dolar", "$");
                totalDolares += master.SubAmount;
            }

            dto.CustomerResponse = await _repositoryTbCustomer.PosMeFindEntityId(master.EntityId);
            dto.FirstName = dto.CustomerResponse.FirstName;
            dto.LastName = dto.CustomerResponse.LastName;
            var findTransactionMasterDetails = await _repositoryTbTransactionMasterDetail.PosMeItemByTransactionId(master.TransactionMasterId);
            foreach (var detail in findTransactionMasterDetails)
            {
                var findItem = await _repositoryItems.PosMeFindByItemId(detail.ComponentItemId);
                findItem.Importe = detail.SubAmount;
                findItem.Quantity = detail.Quantity;
                findItem.PrecioPublico = detail.UnitaryPrice;
                dto.Items.Add(findItem);
            }

            Facturas.Add(dto);
        }

        TotalCordobasFacturado = $"C$ {totalCordobas:N2}";
        TotalDolaresFacturado = $"$ {totalDolares:N2}";
    }
}