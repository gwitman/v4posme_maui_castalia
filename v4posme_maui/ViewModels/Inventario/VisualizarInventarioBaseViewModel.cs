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
    protected readonly IRepositoryParameters RepositoryParameters;

    protected VisualizarInventarioBaseViewModel()
    {
        Helper                 = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        RepositoryItems        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        RepositoryMaster       = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        RepositoryMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        ParameterSystem        = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        RepositoryParameters   = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();

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

    // Logo de la empresa (icono de "mi mercadito") mostrado en la parte superior.
    private ImageSource? _logoSource;
    public ImageSource? LogoSource
    {
        get => _logoSource;
        set => SetProperty(ref _logoSource, value);
    }

    // Datos de la empresa para mostrar nombre y direccion (abajo).
    public string CompanyName => VariablesGlobales.TbCompany?.Name ?? string.Empty;
    public string CompanyAddress => VariablesGlobales.TbCompany?.Address ?? string.Empty;

    private string _companyTelefono = string.Empty;
    public string CompanyTelefono
    {
        get => _companyTelefono;
        set => SetProperty(ref _companyTelefono, value);
    }

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;

        // Esta pantalla es SOLO de visualizacion. La transaccion ya fue insertada en el
        // paso de confirmacion (RevisarProductosInventarioBaseViewModel.OnConfirmar) o se
        // esta cargando un registro existente desde Impresiones. Aqui no se inserta nada.
        TransactionMaster = VariablesGlobales.DtoInventario.TransactionMaster;

        // Cargar logo de la empresa (icono) y telefono para el pie de pagina, igual que
        // la visualizacion de factura.
        try
        {
            var logo = await ParameterSystem.PosMeFindLogo();
            if (!string.IsNullOrWhiteSpace(logo.Value))
            {
                var logoBytes = Convert.FromBase64String(logo.Value!);
                LogoSource    = ImageSource.FromStream(() => new MemoryStream(logoBytes));
            }

            var telefono    = await RepositoryParameters.PosMeFindByKey("CORE_PHONE");
            CompanyTelefono = telefono?.Value ?? string.Empty;

            OnPropertyChanged(nameof(CompanyName));
            OnPropertyChanged(nameof(CompanyAddress));
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
        }

        IsBusy = false;
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
