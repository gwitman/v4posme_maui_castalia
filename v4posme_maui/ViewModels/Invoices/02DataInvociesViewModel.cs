using System.Collections.ObjectModel;
using System.Web;
using System.Windows.Input;
using Android.Media;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.DataGrid;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;
using Unity;
using Microsoft.Maui.Controls.Compatibility.Platform.Android;

namespace v4posme_maui.ViewModels.Invoices;

public class DataInvoicesViewModel : BaseViewModel, IQueryAttributable
{
    private IRepositoryTbCustomer _repositoryTbCustomer;
    private IRepositoryTbCatalogItem _repositoryTbCatalogItem;
    private HelperCore _helperCore;
    private IRepositoryTbZone _repositoryTbZone;
    private IRepositoryItems _repositoryItems;
    private bool _commentRequired   = false;

    public DataInvoicesViewModel()
    {
        _repositoryTbCustomer   = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryTbCatalogItem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCatalogItem>();
        _helperCore             = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        Title                   = "Datos de facturacion 2/6";
        Item                    = VariablesGlobales.DtoInvoice;
        Comentarios             = Item.Comentarios is null ? Comentarios : Item.Comentarios!;
        _repositoryTbZone       = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbZone>();
        _repositoryItems        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        // Se restauran los datos adicionales si el usuario regresa a esta pantalla.
        Referencias                 = Item.Referencia ?? string.Empty;
        ReferenceClientName         = Item.ReferenceClientName ?? string.Empty;
        ReferenceClientIdentifier   = Item.ReferenceClientIdentifier ?? string.Empty;
        NumberPhone                 = Item.NumberPhone ?? string.Empty;

        AbrirCargarSolicitudCommand     = new Command(OnAbrirCargarSolicitud);
        CancelarCargarSolicitudCommand  = new Command(OnCancelarCargarSolicitud);
        CargarSolicitudCommand          = new Command(OnCargarSolicitud);
        SeleccionarInvoiceCreditCommand = new Command(OnSeleccionarDataCredit, ValidateFields);
        PropertyChanged         += (_, _) => SeleccionarInvoiceCreditCommand.ChangeCanExecute();
        LoadComboBox();
        _ = LoadParametersAsync();
    }

    private async Task LoadParametersAsync()
    {
        var value        = await _helperCore.GetValueParameter(Constantes.InvoiceInMobileFieldCommentRequired, "false");
        _commentRequired = value.Trim().ToLower() == "true";
        SeleccionarInvoiceCreditCommand.ChangeCanExecute();

        // "Cargar solicitud" solo se muestra si el servidor lo habilita (por defecto oculto).
        var mostrarSolicitud    = await _helperCore.GetValueParameter(Constantes.InvoiceMobileShowLoadOrder, "false");
        MostrarCargarSolicitud  = mostrarSolicitud.Trim().ToLower() == "true";

        // Rótulos iguales a la web (parámetros copiados de tb_company_page_setting por script SQL).
        EtiquetaComentario      = await _helperCore.GetValueParameterTrim(Constantes.AppMobileLabelCommentInvoice, Mensajes.EtiquetaComentarioFactura);
        EtiquetaZona            = await _helperCore.GetValueParameterTrim(Constantes.AppMobileLabelZoneInvoice, Mensajes.EtiquetaZonaFactura);
    }

    // Copia lo capturado en pantalla al DTO de la factura (se usa al avanzar y al cargar una solicitud).
    private void GuardarDatosEnItem()
    {
        Item.Comentarios                = Comentarios;
        Item.Referencia                 = Referencias;
        Item.ReferenceClientName        = ReferenceClientName;
        Item.ReferenceClientIdentifier  = ReferenceClientIdentifier?.Trim() ?? string.Empty;
        Item.NumberPhone                = NumberPhone?.Trim() ?? string.Empty;
        Item.Zona                       = SelectedZona is not null && SelectedZona.Key > 0 ? SelectedZona : null;
    }

    private void OnAbrirCargarSolicitud()
    {
        NumeroSolicitud             = string.Empty;
        MensajeSolicitud            = string.Empty;
        MostrarPopupCargarSolicitud = true;
    }

    private void OnCancelarCargarSolicitud()
    {
        if (CargandoSolicitud) return;
        MostrarPopupCargarSolicitud = false;
    }

    // Cargar solicitud: requiere internet (consulta la orden en el servidor). Los productos se
    // agregan con el precio local del celular, igual que fnCargarItemSolicitud de la web.
    private async void OnCargarSolicitud()
    {
        if (CargandoSolicitud) return;

        var numero = HelperLoadOrder.NormalizarNumero(NumeroSolicitud);
        if (string.IsNullOrEmpty(numero))
        {
            MensajeSolicitud = Mensajes.MensajeCargarSolicitudIngreseNumero;
            return;
        }

        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            MensajeSolicitud = Mensajes.MensajeCargarSolicitudSinConexion;
            return;
        }

        try
        {
            CargandoSolicitud   = true;
            MensajeSolicitud    = Mensajes.MensajeCargarSolicitudCargando;

            var respuesta = await new RestApiAppMobileApi().GetDataLoadOrder(numero);
            if (respuesta.Error || respuesta.Data is null)
            {
                MensajeSolicitud = string.IsNullOrWhiteSpace(respuesta.Message) ? Mensajes.MensajeCargarSolicitudError : respuesta.Message;
                return;
            }

            var data                    = respuesta.Data;
            ReferenceClientName         = HelperLoadOrder.ValorOActual(data.ReferenceClientName, ReferenceClientName);
            ReferenceClientIdentifier   = HelperLoadOrder.ValorOActual(data.ReferenceClientIdentifier, ReferenceClientIdentifier);
            NumberPhone                 = HelperLoadOrder.ValorOActual(data.NumberPhone, NumberPhone);
            GuardarDatosEnItem();

            var items = HelperLoadOrder.ConsolidarItems(data.Items);
            if (items.Count == 0)
            {
                MostrarPopupCargarSolicitud = false;
                ShowToast(string.Format(Mensajes.MensajeCargarSolicitudSinProductos, numero), ToastDuration.Long, 14);
                return;
            }

            var noEncontrados = await AgregarItemsSolicitudAsync(items);
            var agregados     = items.Count - noEncontrados.Count;

            MostrarPopupCargarSolicitud = false;
            var mensaje = string.Format(Mensajes.MensajeCargarSolicitudOk, numero, agregados);
            if (noEncontrados.Count > 0)
                mensaje += "\n" + string.Format(Mensajes.MensajeCargarSolicitudProductosNoEncontrados, string.Join(", ", noEncontrados));
            ShowToast(mensaje, ToastDuration.Long, 14);
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
            MensajeSolicitud = Mensajes.MensajeCargarSolicitudError;
        }
        finally
        {
            CargandoSolicitud = false;
        }
    }

    // Agrega los productos a la cesta con la misma regla de 04SeleccionarProducto
    // (MOBILE_ALLOW_REPEATED_PRODUCTS / fusion por ItemNumber). Devuelve los ItemID no encontrados.
    private async Task<List<int>> AgregarItemsSolicitudAsync(List<DtoLoadOrderItem> items)
    {
        var noEncontrados       = new List<int>();
        var permitirRepetidos   = await _helperCore.GetValueParameter("MOBILE_ALLOW_REPEATED_PRODUCTS", "false");
        var cestaArticulos      = VariablesGlobales.DtoInvoice.Items;
        var monedaSimbolo       = SelectedCurrency?.Simbolo ?? VariablesGlobales.DtoInvoice.Currency?.Simbolo ?? string.Empty;
        // Base unica para los IDs de detalle: GetTimestampId puede repetirse dentro de un mismo milisegundo.
        var baseDetalleId       = _helperCore.GetTimestampId();
        var indice              = 0;

        foreach (var linea in items)
        {
            var producto = await _repositoryItems.PosMeFindByItemId(linea.ItemID);
            if (producto is null)
            {
                noEncontrados.Add(linea.ItemID);
                continue;
            }

            var cantidad    = linea.Quantity;
            var detalleId   = baseDetalleId + indice++;
            var find        = permitirRepetidos == "true"
                ? null
                : cestaArticulos.FirstOrDefault(response => response.ItemNumber == producto.ItemNumber);

            if (find is not null)
            {
                find.Quantity       += cantidad;
                find.Importe        = find.PrecioPublico * find.Quantity;
                find.MontoDescuento = find.PorcentajeDescuento > 0
                    ? find.Importe * (find.PorcentajeDescuento / 100m)
                    : find.MontoDescuento;
                continue;
            }

            producto.TransactionMasterDetailID  = detalleId;
            producto.MonedaSimbolo              = monedaSimbolo;
            producto.Quantity                   = cantidad;
            producto.Importe                    = producto.PrecioPublico * cantidad;
            producto.MontoDescuento             = 0m;
            producto.PorcentajeDescuento        = 0m;
            cestaArticulos.Add(producto);
        }

        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada = (int)cestaArticulos.Sum(r => r.Quantity);
        VariablesGlobales.DtoInvoice.Balance = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);
        return noEncontrados;
    }

    private bool ValidateFields()
    {
        if (_commentRequired)
            return !string.IsNullOrWhiteSpace(Comentarios);
        return true;
    }

    private async void OnSeleccionarDataCredit()
    {
        if (!ValidateFields())
        {
            return;
        }

        IsBusy = true;
        if (SelectedCurrency is null)
        {
            ShowToast(Mensajes.MensajeSeleccionarMoneda, ToastDuration.Long, 16);
            IsBusy = false;
            return;
        }

        if (SelectedTipoDocumento is null)
        {
            ShowToast(Mensajes.MensajeSeleccionarTipoDocumento, ToastDuration.Long, 16);
            IsBusy = false;
            return;
        }

        GuardarDatosEnItem();
        Item.Currency               = SelectedCurrency;
        Item.TipoDocumento          = SelectedTipoDocumento;
        Item.Mesa                   = SelectedMesa;

        // Flujo secuencial: al dar "Mas opciones" SIEMPRE se avanza a la pantalla de datos
        // de credito (3/6).
        var customerNumber = VariablesGlobales.DtoInvoice.CustomerResponse?.CustomerNumber
                             ?? VariablesGlobales.DtoInvoice.CustomerNumber;
        await NavigationService.NavigateToAsync<DataInvoiceCreditViewModel>(customerNumber!);
        IsBusy = false;
    }

    public bool ErrorCurrency { get; set; }
    public bool ErrorTipoDocumento { get; set; }
    public bool ErrorComentarios { get; set; }
    public bool ErrorReferencia { get; set; }
    public bool ErrorReferenceClientName { get; set; }
    public bool ErrorMesa { get; set; }
    public ViewTempDtoInvoice Item { get; private set; }
    private string _comentarios="Sin Comentarios";

    public string Comentarios
    {
        get => _comentarios;
        set => SetProperty(ref _comentarios, value);
    }

    private string _referencias=string.Empty;


    public string Referencias
    {
        get => _referencias;
        set => SetProperty(ref _referencias, value);
    }

    private string _ReferenceClientName = string.Empty;
    public string ReferenceClientName
    {
        get => _ReferenceClientName;
        set => SetProperty(ref _ReferenceClientName, value);
    }

    private ObservableCollection<DtoCatalogItem>? _currencies;

    public ObservableCollection<DtoCatalogItem>? Currencies
    {
        get => _currencies;
        set => SetProperty(ref _currencies, value);
    }

    private ObservableCollection<DtoCatalogItem>? _mesas;
    public ObservableCollection<DtoCatalogItem>? Mesas
    {
        get => _mesas;
        set => SetProperty(ref _mesas, value);
    }

    private ObservableCollection<DtoCatalogItem>? _tipoDocumentos;

    public ObservableCollection<DtoCatalogItem>? TipoDocumentos
    {
        get => _tipoDocumentos;
        set => SetProperty(ref _tipoDocumentos, value);
    }

    public Command SeleccionarInvoiceCreditCommand { get; }
   

    private DtoCatalogItem? _selectedCurrency;
    public DtoCatalogItem? SelectedCurrency
    {
        get => _selectedCurrency;
        set => SetProperty(ref _selectedCurrency, value);
    }

    private DtoCatalogItem? _selectedTipoDocumento;
    public DtoCatalogItem? SelectedTipoDocumento
    {
        get => _selectedTipoDocumento;
        set => SetProperty(ref _selectedTipoDocumento, value);
    }

    private DtoCatalogItem? _selectedMesa;
    public DtoCatalogItem? SelectedMesa
    {
        get => _selectedMesa;
        set => SetProperty(ref _selectedMesa, value);
    }

    // Datos adicionales de facturacion (opcionales, mismos campos que la web ext_6_0_2)
    private string _referenceClientIdentifier = string.Empty;
    public string ReferenceClientIdentifier
    {
        get => _referenceClientIdentifier;
        set => SetProperty(ref _referenceClientIdentifier, value);
    }

    private string _numberPhone = string.Empty;
    public string NumberPhone
    {
        get => _numberPhone;
        set => SetProperty(ref _numberPhone, value);
    }

    private ObservableCollection<DtoCatalogItem>? _zonas;
    public ObservableCollection<DtoCatalogItem>? Zonas
    {
        get => _zonas;
        set => SetProperty(ref _zonas, value);
    }

    private DtoCatalogItem? _selectedZona;
    public DtoCatalogItem? SelectedZona
    {
        get => _selectedZona;
        set => SetProperty(ref _selectedZona, value);
    }

    // Cargar solicitud
    public Command AbrirCargarSolicitudCommand { get; }
    public Command CancelarCargarSolicitudCommand { get; }
    public Command CargarSolicitudCommand { get; }

    // Rótulos configurables (mismos que la web)
    private string _etiquetaComentario = Mensajes.EtiquetaComentarioFactura;
    public string EtiquetaComentario
    {
        get => _etiquetaComentario;
        set => SetProperty(ref _etiquetaComentario, value);
    }

    private string _etiquetaZona = Mensajes.EtiquetaZonaFactura;
    public string EtiquetaZona
    {
        get => _etiquetaZona;
        set => SetProperty(ref _etiquetaZona, value);
    }

    private bool _mostrarCargarSolicitud;
    public bool MostrarCargarSolicitud
    {
        get => _mostrarCargarSolicitud;
        set => SetProperty(ref _mostrarCargarSolicitud, value);
    }

    private bool _mostrarPopupCargarSolicitud;
    public bool MostrarPopupCargarSolicitud
    {
        get => _mostrarPopupCargarSolicitud;
        set => SetProperty(ref _mostrarPopupCargarSolicitud, value);
    }

    private string _numeroSolicitud = string.Empty;
    public string NumeroSolicitud
    {
        get => _numeroSolicitud;
        set => SetProperty(ref _numeroSolicitud, value);
    }

    private bool _cargandoSolicitud;
    public bool CargandoSolicitud
    {
        get => _cargandoSolicitud;
        set => SetProperty(ref _cargandoSolicitud, value);
    }

    private string _mensajeSolicitud = string.Empty;
    public string MensajeSolicitud
    {
        get => _mensajeSolicitud;
        set => SetProperty(ref _mensajeSolicitud, value);
    }


    public void OnAppearing(INavigation? navigation)
    {
        Navigation = navigation;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var id = HttpUtility.UrlDecode(query["id"] as string);
        await LoadData(id);
    }

    private async Task LoadData(string? id)
    {
        var customer    = await _repositoryTbCustomer.PosMeFindCustomer(id!);
        Item            = VariablesGlobales.DtoInvoice;
        VariablesGlobales.DtoInvoice.CustomerResponse = customer;
        LoadComboBox();
        IsBusy = false;
    }

    private void LoadComboBox()
    {
        Currencies =
        [
            new DtoCatalogItem((int)TypeCurrency.Cordoba, "Córdobas", "C$"),
            new DtoCatalogItem((int)TypeCurrency.Dolar, "Dolares", "$")
        ];
        TipoDocumentos =
        [
            new DtoCatalogItem((int)TypeTransactionCausal.Contado, "Contado", "D"),
            new DtoCatalogItem((int)TypeTransactionCausal.Credito, "Crédito", "C")
        ];

        //Llenar el catalogo de mesas
        Mesas                                                                       =  [ new DtoCatalogItem(0, "Seleccione", "Seleccione")];
        List<Api_AppMobileApi_GetDataDownloadCatalogItemResponse> catalogoDeMesas   = _repositoryTbCatalogItem.PosMeFindByName( Constantes.CatalogName_MESAS).Result;
        if (catalogoDeMesas != null)
        {
            if(catalogoDeMesas.Count > 0)
            {
                foreach(var catalogoDeMesasI in catalogoDeMesas)
                {
                    Mesas.Add(new DtoCatalogItem(catalogoDeMesasI.catalogItemID, catalogoDeMesasI.catalogItemName, catalogoDeMesasI.catalogItemDisplay));
                }
            }
        }


        if(Mesas.Any())
        {
            SelectedMesa = Mesas.First();
        }
        if(VariablesGlobales.DtoInvoice.Mesa != null)
        {
            SelectedMesa = Mesas.Where(k => k.Key == VariablesGlobales.DtoInvoice.Mesa.Key).FirstOrDefault();
        }

        // Catalogo de zonas (tb_transaction_master_info_billing.zoneID), descargado en tb_zone para uso sin internet.
        Zonas = [ new DtoCatalogItem(0, "Seleccione", "Seleccione") ];
        var catalogoDeZonas = _repositoryTbZone.PosMeFindAllOrdered().Result;
        if (catalogoDeZonas != null)
        {
            foreach (var zonaI in catalogoDeZonas)
            {
                Zonas.Add(new DtoCatalogItem(zonaI.CatalogItemID, zonaI.Name ?? string.Empty, zonaI.Display ?? string.Empty));
            }
        }
        SelectedZona = Zonas.First();
        if (VariablesGlobales.DtoInvoice.Zona != null)
        {
            SelectedZona = Zonas.FirstOrDefault(k => k.Key == VariablesGlobales.DtoInvoice.Zona.Key) ?? Zonas.First();
        }

        if (Currencies.Any())
        {
            SelectedCurrency = Currencies.First();
        }
        if (VariablesGlobales.DtoInvoice.Currency != null)
        {
            SelectedCurrency = Currencies.Where(k => k.Key == VariablesGlobales.DtoInvoice.Currency.Key).FirstOrDefault();
        }

        if (TipoDocumentos.Any())
        {
            SelectedTipoDocumento = TipoDocumentos.First();
        }
        if (VariablesGlobales.DtoInvoice.TipoDocumento != null)
        {
            SelectedTipoDocumento = TipoDocumentos.Where(k => k.Key == VariablesGlobales.DtoInvoice.TipoDocumento.Key).FirstOrDefault();
        }

    }
}