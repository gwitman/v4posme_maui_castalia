using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Inventario;

// Base compartida para el paso de revision de cantidades, precios y costos en los
// flujos de inventario. Los editores se enlazan directamente a cada producto; este
// view model recalcula el total, PERSISTE la transaccion al confirmar y navega a la
// pantalla de visualizacion (que es solo lectura).
public abstract class RevisarProductosInventarioBaseViewModel : BaseViewModel
{
    protected readonly HelperCore Helper;
    protected readonly IRepositoryItems RepositoryItems;
    protected readonly IRepositoryTbTransactionMaster RepositoryMaster;
    protected readonly IRepositoryTbTransactionMasterDetail RepositoryMasterDetail;

    protected RevisarProductosInventarioBaseViewModel()
    {
        Helper                 = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        RepositoryItems        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        RepositoryMaster       = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        RepositoryMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();

        ProductosSeleccionados = VariablesGlobales.DtoInventario.Items;
        RecalcularCommand      = new Command(RecalcularTotales);
        ConfirmarCommand       = new Command(OnConfirmar);
        AtrasCommand           = new Command(OnAtras);
    }

    // Texto del boton confirmar (Confirmar Compra / Confirmar Salida).
    public abstract string TextoConfirmar { get; }

    // Indica si en este flujo se puede editar el precio publico. En la Entrada se
    // permiten cantidad, costo y precio; en la Salida solo cantidad.
    public abstract bool PermiteEditarPrecio { get; }
    public abstract bool PermiteEditarCosto { get; }

    // Tipo de transaccion de este flujo.
    protected abstract TypeTransaction TipoTransaccion { get; }

    // true si es Entrada (aumenta CantidadEntradas); false si es Salida (aumenta CantidadSalidas).
    protected abstract bool EsEntrada { get; }

    // Genera el codigo correspondiente al flujo.
    protected abstract string GenerarCodigo();

    // Navegacion a la pantalla de visualizacion (implementada por cada flujo).
    protected abstract Task NavegarAVisualizacionAsync();

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> ProductosSeleccionados { get; }

    public Command RecalcularCommand { get; }
    public Command ConfirmarCommand { get; }
    public Command AtrasCommand { get; }

    public string MonedaSimbolo => "C$";

    // Solo visualizacion: el total mostrado es la suma de cantidad x costo.
    public decimal Total => ProductosSeleccionados.Sum(r => r.Cost * r.Quantity);

    public int CantidadTotalItems => (int)ProductosSeleccionados.Sum(r => r.Quantity);

    protected void RecalcularTotales()
    {
        // Eliminar de la lista los productos cuya cantidad quede en 0 (o menos) y
        // actualizar los contadores y totales del DTO.
        var aEliminar = ProductosSeleccionados.Where(r => r.Quantity <= 0).ToList();
        foreach (var item in aEliminar)
        {
            ProductosSeleccionados.Remove(item);
        }

        foreach (var item in ProductosSeleccionados)
        {
            // Solo visualizacion: el importe mostrado es cantidad x costo.
            item.Importe = item.Cost * item.Quantity;
        }

        VariablesGlobales.DtoInventario.Balance                   = ProductosSeleccionados.Sum(r => r.Importe);
        VariablesGlobales.DtoInventario.CantidadTotalSeleccionada = (int)ProductosSeleccionados.Sum(r => r.Quantity);
        OnPropertyChanged(nameof(Total));
        OnPropertyChanged(nameof(CantidadTotalItems));
    }

    private async void OnAtras()
    {
        await NavigationService.GoBackAsync();
    }

    // Etiqueta legible del flujo usada en los logs (Compra / Otra Entrada / Salida / Ajuste).
    // Cada flujo la sobreescribe; por defecto se deriva del tipo de transacción.
    protected virtual string EtiquetaLog => TipoTransaccion.ToString();

    // Nombre de pantalla para los logs. Incluye el tipo de transacción (Ajuste/Entrada/Salida/Compra).
    private string Screen => $"Inventario-{EtiquetaLog}";

    private async void OnConfirmar()
    {
        HelperLogs.Trace(Screen, "OnConfirmar", "inicio");
        RecalcularTotales();

        if (ProductosSeleccionados.Count == 0 || ProductosSeleccionados.Any(p => p.Quantity <= 0))
        {
            HelperLogs.Trace(Screen, "OnConfirmar", $"validación fallida: {ProductosSeleccionados.Count} productos, hay cantidades <= 0", "Warning");
            ShowToast("Las cantidades deben ser mayores a cero", ToastDuration.Long, 12);
            return;
        }

        try
        {
            IsBusy = true;

            // Persistir la transaccion AQUI (una sola vez, al confirmar). La pantalla de
            // visualizacion solo mostrara el registro ya guardado.
            HelperLogs.Trace(Screen, "OnConfirmar", $"guardando transacción con {ProductosSeleccionados.Count} productos");
            await GuardarAsync();

            HelperLogs.Trace(Screen, "OnConfirmar", "guardado con éxito, navegando a visualización");
            await NavegarAVisualizacionAsync();
        }
        catch (Exception ex)
        {
            HelperLogs.Trace(Screen, "OnConfirmar", $"EXCEPCIÓN: {ex.GetType().Name} - {ex.Message}", "Error");
            HelperLogs.Log(ex);
            ShowToast($"Error al confirmar: {ex.Message}", ToastDuration.Long, 12);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task GuardarAsync()
    {
        var dto           = VariablesGlobales.DtoInventario;
        HelperLogs.TraceValue(Screen, "VariablesGlobales.DtoInventario", dto);
        HelperLogs.TraceValue(Screen, "VariablesGlobales.User", VariablesGlobales.User);
        if (dto is null)
            throw new InvalidOperationException("VariablesGlobales.DtoInventario es null. No hay transacción de inventario activa.");
        if (VariablesGlobales.User is null)
            throw new InvalidOperationException("VariablesGlobales.User es null. No hay sesión de usuario activa.");

        var codigo        = GenerarCodigo();
        HelperLogs.TraceValue(Screen, "codigo", codigo);
        dto.Codigo        = codigo;
        dto.TransactionOn = DateTime.Now;

        var master = new TbTransactionMaster
        {
            TransactionId     = TipoTransaccion,
            TransactionNumber = codigo,
            TransactionOn     = dto.TransactionOn,
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

        HelperLogs.DumpObject(Screen, "master", master);
        HelperLogs.Trace(Screen, "GuardarAsync", $"insertando transacción maestra (codigo={codigo}, Amount={master.Amount})");
        await RepositoryMaster.PosMeInsert(master);
        var masterId = master.TransactionMasterId;
        HelperLogs.Trace(Screen, "GuardarAsync", $"maestra insertada (TransactionMasterId={masterId}), construyendo {dto.Items.Count} detalles");

        var detalles = new List<TbTransactionMasterDetail>();
        foreach (var item in dto.Items)
        {
            var detalle = new TbTransactionMasterDetail
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
            };
            HelperLogs.DumpObject(Screen, $"detalle(ItemId={item.ItemId})", detalle);
            detalles.Add(detalle);

            await AjustarCantidadProductoAsync(item);
        }

        HelperLogs.Trace(Screen, "GuardarAsync", $"insertando {detalles.Count} detalles y actualizando contador");
        await RepositoryMasterDetail.PosMeInsertAll(detalles);
        await Helper.PlusCounter();

        dto.TransactionMasterId     = masterId;
        dto.TransactionMaster       = master;
        dto.AbiertoDesdeImpresiones = false;
        HelperLogs.Trace(Screen, "GuardarAsync", "finalizado");
    }

    // Ajusta CantidadEntradas / CantidadSalidas del producto y recalcula CantidadFinal.
    // En una Entrada (Compra) tambien actualiza el precio publico y el costo del item con
    // los valores capturados en la revision.
    private async Task AjustarCantidadProductoAsync(Api_AppMobileApi_GetDataDownloadItemsResponse item)
    {
        var producto = await RepositoryItems.PosMeFindByItemId(item.ItemId);
        if (producto is null) return;

        if (EsEntrada)
        {
            producto.CantidadEntradas += item.Quantity;
            // Actualizar precio publico y costo del item con lo ingresado en la compra.
            producto.PrecioPublico = item.PrecioPublico;
            producto.Cost          = item.Cost;
        }
        else
        {
            producto.CantidadSalidas += item.Quantity;
        }

        producto.CantidadFinal = (producto.Quantity + producto.CantidadEntradas)
                                 - (producto.CantidadSalidas + producto.CantidadFacturadas);
        await RepositoryItems.PosMeUpdate(producto);
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        RecalcularTotales();
        IsBusy = false;
    }
}
