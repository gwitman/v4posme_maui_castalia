using System.Collections.ObjectModel;
using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using Plugin.BLE;
using v4posme_maui.Models;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.HelpersPrinters.Helper;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Inventario;

// Base compartida para la visualizacion (modo lectura) de una transaccion de inventario.
// Al aparecer por primera vez inserta la transaccion (master + detalle), aumenta el
// contador local y ajusta las cantidades de los productos:
//   - Entrada (Compras): aumenta CantidadEntradas
//   - Salida (Otras salidas): aumenta CantidadSalidas
// Expone los botones: Nueva, Imprimir, Compartir, Eliminar.
public abstract class VisualizarInventarioBaseViewModel : BaseViewModel
{
    protected readonly HelperCore Helper;
    protected readonly IRepositoryItems RepositoryItems;
    protected readonly IRepositoryTbTransactionMaster RepositoryMaster;
    protected readonly IRepositoryTbTransactionMasterDetail RepositoryMasterDetail;
    protected readonly IRepositoryTbParameterSystem ParameterSystem;
    private bool _guardado;

    protected VisualizarInventarioBaseViewModel()
    {
        Helper                 = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        RepositoryItems        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        RepositoryMaster       = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        RepositoryMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        ParameterSystem        = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();

        NuevaCommand              = new Command(OnNueva);
        ImprimirCommand           = new Command(OnImprimir);
        CompartirCommand          = new Command(OnCompartir);
        EliminarCommand           = new Command(OnEliminar);
        AbrirMenuPrincipalCommand = new Command(OnAbrirMenuPrincipal);
        RegresarCommand           = new Command(OnRegresar);
    }

    // El icono superior izquierdo (drawer) abre el menu principal (flyout) del Shell.
    public Command AbrirMenuPrincipalCommand { get; }

    // Comando del boton atras cuando la pantalla se abre desde Impresiones (solo lectura).
    public Command RegresarCommand { get; }

    // true cuando la pantalla se abrio desde Impresiones (listado). En ese caso se debe
    // permitir regresar al listado en lugar de comportarse como paso final del flujo.
    public bool AbiertoDesdeImpresiones => VariablesGlobales.DtoInventario.AbiertoDesdeImpresiones;

    private void OnAbrirMenuPrincipal()
    {
        if (Shell.Current is not null)
            Shell.Current.FlyoutIsPresented = true;
    }

    private async void OnRegresar()
    {
        // Regresa al listado de Impresiones (elimina esta pagina de la pila).
        await Navigation!.PopAsync();
    }

    // Tipo de transaccion de este flujo.
    protected abstract TypeTransaction TipoTransaccion { get; }

    // true si es Entrada (aumenta CantidadEntradas); false si es Salida (aumenta CantidadSalidas).
    protected abstract bool EsEntrada { get; }

    // Genera el codigo correspondiente al flujo.
    protected abstract string GenerarCodigo();

    // Ruta (flyout) del primer paso del flujo para iniciar una nueva transaccion.
    protected abstract string RutaNuevo { get; }

    public Command NuevaCommand { get; }
    public Command ImprimirCommand { get; }
    public Command CompartirCommand { get; }
    public Command EliminarCommand { get; }

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Items => VariablesGlobales.DtoInventario.Items;
    public string Codigo => VariablesGlobales.DtoInventario.Codigo;
    public string Comentarios => VariablesGlobales.DtoInventario.Comentarios ?? string.Empty;
    public string Referencia1 => VariablesGlobales.DtoInventario.Referencia1 ?? string.Empty;
    public string Referencia2 => VariablesGlobales.DtoInventario.Referencia2 ?? string.Empty;
    public string MonedaSimbolo => "C$";
    public string FechaTexto => VariablesGlobales.DtoInventario.TransactionOn.ToString("dd/MM/yyyy HH:mm");
    public string Total => VariablesGlobales.DtoInventario.Balance.ToString("N2");
    public int CantidadTotalItems => (int)Items.Sum(r => r.Quantity);

    private TbTransactionMaster? _transactionMaster;
    public TbTransactionMaster? TransactionMaster
    {
        get => _transactionMaster;
        set => SetProperty(ref _transactionMaster, value);
    }

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;

        // Se guarda una sola vez (al llegar desde la confirmacion). Si el usuario navega
        // hacia atras y adelante no se vuelve a insertar.
        if (!_guardado && VariablesGlobales.DtoInventario.TransactionMasterId <= 0)
        {
            await GuardarAsync();
        }
        else
        {
            TransactionMaster = VariablesGlobales.DtoInventario.TransactionMaster;
        }

        IsBusy = false;
    }

    private async Task GuardarAsync()
    {
        try
        {
            IsBusy = true;

            var codigo    = GenerarCodigo();
            var dto       = VariablesGlobales.DtoInventario;
            dto.Codigo    = codigo;
            dto.TransactionOn = DateTime.Now;

            var master = new TbTransactionMaster
            {
                TransactionId     = TipoTransaccion,
                TransactionNumber = codigo,
                TransactionOn     = DateTime.Now,
                EntitySecondaryId = VariablesGlobales.User!.UserId.ToString(),
                Comment           = dto.Comentarios,
                Reference1        = dto.Referencia1,
                Reference2        = dto.Referencia2,
                CurrencyId        = TypeCurrency.Cordoba,
                SubAmount         = dto.Items.Sum(p => p.PrecioPublico * p.Quantity),
                Amount            = dto.Items.Sum(p => p.PrecioPublico * p.Quantity),
                Discount          = decimal.Zero,
                Taxi1             = decimal.Zero,
                ExchangeRate      = decimal.Zero,
                StatusID          = (int)TypeStatusBilling.Register,
                RegisterLocal     = 1
            };

            await RepositoryMaster.PosMeInsert(master);
            var masterId = master.TransactionMasterId;

            var detalles = new List<TbTransactionMasterDetail>();
            foreach (var item in dto.Items)
            {
                detalles.Add(new TbTransactionMasterDetail
                {
                    TransactionMasterId = masterId,
                    Componentid         = (int)TypeComponent.Itme,
                    ComponentItemId     = item.ItemId,
                    Quantity            = item.Quantity,
                    UnitaryCost         = item.Cost,
                    UnitaryPrice        = item.PrecioPublico,
                    SubAmount           = item.PrecioPublico * item.Quantity,
                    Amount              = item.PrecioPublico * item.Quantity,
                    Discount            = decimal.Zero,
                    Tax1                = decimal.Zero,
                    ItemBarCode         = item.BarCode,
                    RegisterLocal       = 1
                });

                // Ajustar la existencia del producto.
                await AjustarCantidadProductoAsync(item, sumar: true);
            }

            await RepositoryMasterDetail.PosMeInsertAll(detalles);
            await Helper.PlusCounter();

            dto.TransactionMasterId = masterId;
            dto.TransactionMaster   = master;
            TransactionMaster       = master;
            _guardado               = true;

            OnPropertyChanged(nameof(Codigo));
            ShowMensajePopUp(EsEntrada ? "Compra registrada correctamente" : "Salida registrada correctamente", Colors.Green);
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Ajusta CantidadEntradas / CantidadSalidas del producto y recalcula CantidadFinal.
    // sumar=true suma la cantidad del item; sumar=false la resta (al eliminar).
    private async Task AjustarCantidadProductoAsync(Api_AppMobileApi_GetDataDownloadItemsResponse item, bool sumar)
    {
        var producto = await RepositoryItems.PosMeFindByItemId(item.ItemId);
        if (producto is null) return;

        var delta = sumar ? item.Quantity : -item.Quantity;
        if (EsEntrada)
            producto.CantidadEntradas += delta;
        else
            producto.CantidadSalidas += delta;

        producto.CantidadFinal = (producto.Quantity + producto.CantidadEntradas)
                                 - (producto.CantidadSalidas + producto.CantidadFacturadas);
        await RepositoryItems.PosMeUpdate(producto);
    }

    private async void OnNueva()
    {
        // Reinicia el estado del flujo y regresa al primer paso (Datos) usando la ruta
        // absoluta del flyout. La navegacion absoluta (//) reinicia por completo la pila
        // de navegacion dejando la pantalla de Datos como raiz del flyout, con lo que el
        // icono de menu (hamburguesa/drawer) vuelve a mostrarse correctamente.
        VariablesGlobales.DtoInventario = new ViewTempDtoInventario { TransactionId = TipoTransaccion };
        await Shell.Current.GoToAsync($"//{RutaNuevo}");
    }

    private async void OnEliminar()
    {
        try
        {
            IsBusy = true;
            var masterId = VariablesGlobales.DtoInventario.TransactionMasterId;
            if (masterId <= 0)
            {
                ShowToast("No hay registro que eliminar", ToastDuration.Short, 12);
                IsBusy = false;
                return;
            }

            var master  = await RepositoryMaster.PosMeFindByTransactionId(masterId);
            var detalles = await RepositoryMasterDetail.PosMeItemByTransactionId(masterId);

            // Revertir las cantidades previamente ajustadas.
            foreach (var detalle in detalles)
            {
                var producto = await RepositoryItems.PosMeFindByItemId(detalle.ComponentItemId);
                if (producto is not null)
                {
                    if (EsEntrada)
                        producto.CantidadEntradas -= detalle.Quantity;
                    else
                        producto.CantidadSalidas -= detalle.Quantity;

                    producto.CantidadFinal = (producto.Quantity + producto.CantidadEntradas)
                                             - (producto.CantidadSalidas + producto.CantidadFacturadas);
                    await RepositoryItems.PosMeUpdate(producto);
                }
                await RepositoryMasterDetail.PosMeDelete(detalle);
            }

            if (master is not null)
                await RepositoryMaster.PosMeDelete(master);

            ShowMensajePopUp("Registro eliminado correctamente", Colors.Green);
            OnNueva();
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 13);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnImprimir()
    {
        try
        {
            var parametroPrinter = await ParameterSystem.PosMeFindPrinter();
            if (string.IsNullOrWhiteSpace(parametroPrinter.Value))
            {
                ShowMensajePopUp("No hay impresora configurada");
                return;
            }
            if (!CrossBluetoothLE.Current.IsOn)
            {
                ShowMensajePopUp(Mensajes.MensajeBluetoothState);
                return;
            }

            IsBusy = true;
            var printer = new Printer(parametroPrinter.Value);
            await PrinterInventarioFormatHelper.PrintFormat(
                printer,
                VariablesGlobales.DtoInventario,
                EsEntrada ? "COMPRA" : "SALIDA",
                VariablesGlobales.TbCompany?.Name ?? string.Empty,
                VariablesGlobales.User!.Nickname!);

            if (printer.Device is null)
                ShowMensajePopUp(Mensajes.MensajeDispositivoNoConectado);
            IsBusy = false;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
            ShowMensajePopUp(e.Message);
        }
    }

    // La pagina se suscribe a este evento para capturar la pantalla como imagen y
    // compartirla (igual que factura, abono y gasto).
    public event EventHandler? CompartirSolicitado;

    private void OnCompartir()
    {
        CompartirSolicitado?.Invoke(this, EventArgs.Empty);
    }

    // Titulo usado por la pagina al compartir la imagen.
    public string TituloCompartir => EsEntrada ? "Compartir Compra" : "Compartir Salida";
}
