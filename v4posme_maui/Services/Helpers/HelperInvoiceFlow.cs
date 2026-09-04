using System.Collections.ObjectModel;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

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

        VariablesGlobales.DtoInvoice            = dto;
        VariablesGlobales.InvoiceFlowInicializado = true;
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
