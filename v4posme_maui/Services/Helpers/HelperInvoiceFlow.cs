using System.Collections.ObjectModel;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.Services.Helpers;

/// <summary>
/// Ayuda a inicializar el DtoInvoice con valores por defecto para la facturacion rapida,
/// permitiendo ir directamente a la pantalla de seleccion de producto (4/6) sin pasar
/// por las pantallas de cliente, datos de factura y credito.
/// </summary>
public class HelperInvoiceFlow(IRepositoryTbCustomer repositoryTbCustomer)
{
    private readonly IRepositoryTbCustomer _repositoryTbCustomer = repositoryTbCustomer;

    /// <summary>
    /// Inicializa el DtoInvoice con los valores por defecto solo si el flujo no ha sido
    /// inicializado aun. De esta forma se conservan los productos ya seleccionados al
    /// volver desde el menu desplegable de la pantalla 4/6.
    /// </summary>
    public async Task InicializarFacturaRapidaAsync()
    {
        if (VariablesGlobales.InvoiceFlowInicializado)
        {
            return;
        }

        var customer = await _repositoryTbCustomer.PosMeFindCustomer(VariablesGlobales.ClienteGenericoNumberDefault);

        var dto = new ViewTempDtoInvoice
        {
            CustomerResponse = customer,
            CustomerNumber   = VariablesGlobales.ClienteGenericoNumberDefault,
            FirstName        = customer?.FirstName,
            LastName         = customer?.LastName,
            Balance          = customer?.Balance ?? decimal.Zero,
            Comentarios      = "Sin Comentarios",
            Referencia       = string.Empty,
            // Moneda por defecto: Cordoba = 1
            Currency         = new DtoCatalogItem((int)TypeCurrency.Cordoba, "Córdobas", "C$"),
            // Causal por defecto: Contado = 21
            TipoDocumento    = new DtoCatalogItem((int)TypeTransactionCausal.Contado, "Contado", "D"),
            // Mesa por defecto (opcion "Seleccione", Key = 0)
            Mesa             = new DtoCatalogItem(0, "Seleccione", "Seleccione"),
            // Datos de credito por defecto
            PeriodPay        = new DtoCatalogItem((int)TypePeriodPay.Mensual, "Mensual", "M"),
            Plazo            = 1,
            FixedExpenses    = decimal.Zero,
            NextVisit        = DateTime.Now.Date
        };

        await AplicarValoresPorDefectoAsync(dto);

        VariablesGlobales.DtoInvoice            = dto;
        VariablesGlobales.InvoiceFlowInicializado = true;
    }

    // Opciones de los combos de moneda y tipo de documento (mismas de la pantalla 2/6).
    public static List<DtoCatalogItem> OpcionesMoneda() =>
    [
        new DtoCatalogItem((int)TypeCurrency.Cordoba, "Córdobas", "C$"),
        new DtoCatalogItem((int)TypeCurrency.Dolar, "Dolares", "$")
    ];

    public static List<DtoCatalogItem> OpcionesTipoDocumento() =>
    [
        new DtoCatalogItem((int)TypeTransactionCausal.Contado, "Contado", "D"),
        new DtoCatalogItem((int)TypeTransactionCausal.Credito, "Crédito", "C")
    ];

    /// <summary>
    /// Valores por defecto de APP_MOBILE_SETTING (value_default) para una factura NUEVA.
    /// Se aplican aquí porque todas las facturas nuevas nacen en este método (flujo por
    /// cliente 1/6 y facturación rápida 4/6); la edición desde el voucher no pasa por aquí
    /// y conserva los datos guardados. En los combos se acepta el ID o el nombre.
    /// </summary>
    private static async Task AplicarValoresPorDefectoAsync(ViewTempDtoInvoice dto)
    {
        try
        {
            var helperCore = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
            var setting    = await helperCore.GetAppMobileSettingAsync();
            if (setting.Controles.Count == 0)
                return;

            var moneda = HelperAppMobileSetting.BuscarOpcion(OpcionesMoneda(),
                HelperAppMobileSetting.ValorPorDefecto(setting, Constantes.SettingControlMoneda));
            if (moneda is not null)
                dto.Currency = moneda;

            var tipoDocumento = HelperAppMobileSetting.BuscarOpcion(OpcionesTipoDocumento(),
                HelperAppMobileSetting.ValorPorDefecto(setting, Constantes.SettingControlTipoDocumento));
            if (tipoDocumento is not null)
                dto.TipoDocumento = tipoDocumento;

            var mesaDefecto = HelperAppMobileSetting.ValorPorDefecto(setting, Constantes.SettingControlMesa);
            if (!string.IsNullOrWhiteSpace(mesaDefecto))
            {
                var catalogo = await VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCatalogItem>().PosMeFindByName(Constantes.CatalogName_MESAS);
                var mesas    = catalogo?.Select(m => new DtoCatalogItem(m.catalogItemID, m.catalogItemName, m.catalogItemDisplay));
                var mesa     = HelperAppMobileSetting.BuscarOpcion(mesas, mesaDefecto);
                if (mesa is not null)
                    dto.Mesa = mesa;
            }

            var zonaDefecto = HelperAppMobileSetting.ValorPorDefecto(setting, Constantes.SettingControlZona);
            if (!string.IsNullOrWhiteSpace(zonaDefecto))
            {
                var catalogo = await VariablesGlobales.UnityContainer.Resolve<IRepositoryTbZone>().PosMeFindAllOrdered();
                var zonas    = catalogo?.Select(z => new DtoCatalogItem(z.CatalogItemID, z.Name ?? string.Empty, z.Display ?? string.Empty));
                var zona     = HelperAppMobileSetting.BuscarOpcion(zonas, zonaDefecto);
                if (zona is not null)
                    dto.Zona = zona;
            }

            dto.NumberPhone               = HelperAppMobileSetting.ValorPorDefecto(setting, Constantes.SettingControlTelefono) ?? dto.NumberPhone;
            dto.ReferenceClientIdentifier = HelperAppMobileSetting.ValorPorDefecto(setting, Constantes.SettingControlCedula) ?? dto.ReferenceClientIdentifier;
            dto.ReferenceClientName       = HelperAppMobileSetting.ValorPorDefecto(setting, Constantes.SettingControlClienteReferencia) ?? dto.ReferenceClientName;
        }
        catch (Exception ex)
        {
            // La configuración es opcional: un error no debe impedir facturar.
            HelperLogs.Log(ex);
        }
    }

    /// <summary>
    /// Reinicia el flujo de facturacion rapida para que la proxima entrada vuelva a
    /// generar un DtoInvoice limpio con los valores por defecto.
    /// </summary>
    public static void ReiniciarFlujo()
    {
        VariablesGlobales.InvoiceFlowInicializado = false;
    }
}
