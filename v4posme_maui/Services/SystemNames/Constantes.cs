using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v4posme_maui.Services.SystemNames;

public static class Constantes
{
    public static readonly string ParametroCounter                  = "COUNTER";
    public static readonly string ParametroLogo                     = "LOGO";
    public static readonly string ParametroTema                     = "THEME";
    public static readonly string ParametroAccesPoint               = "ACCESS_POINT";
    public static readonly string ParametroPrinter                  = "PRINTER";
    public static readonly string ParametroCodigoAbono              = "TRANSACTION_SHARE";
    public static readonly string ParemeterEntityIDAutoIncrement    = "AUTO_INCREMENT";
    public const string ParameterCodigoFactura                      = "TRANSACTION_INVOICE";
	public const string ParameterCodigoVisita                       = "TRANSACTION_VISIT";
	public const string ParameterCodigoGasto                        = "TRANSACTION_EXPENSE";
	public const string ParameterCodigoCashInflow                   = "TRANSACTION_CASH_INFLOW";
	public const string ParameterCodigoCashOutflow                  = "TRANSACTION_CASH_OUTFLOW";
	public const string ParametroUserAgent                          = "USER_AGENT";
	public const string DefaultUserAgent                            = "Postme-API/1.0";
	public const string CustomerOrderShare                          = "CUSTOMER_ORDER_SHARE";
	public const string CustomerOrderCustomer                       = "CUSTOMER_ORDER_CUSTOMER";
	public const string CustomerOrderInvoice						= "CUSTOMER_ORDER_INVOICE";
	public static int CompanyId                                     = 2;
    public static int BranchId                                      = 2;
    public static int UserIdAdmin                                   = 2;
    public const string UrlBase                                     = "{UrlBase}";
    public const string UrlBasePosme                                = "https://posme.net/v4posme/posme/public/";
    public const string TimeGpsInMilleseconds                       = "2629824000"; //1 mes
    public const string GpsTitleContentNotification = "Gps posMe";
    public const string GpsTextContentNotification  = "Envio cada 5 min";
    public const string TagGps                      = "GpsLocationServicePosMe";
    public const string GpsServicesChanelId         = "7071";
    public const int GpsServicesId                  = 7070;
    public const string GpsNameServices             = "GpsServicesPosme";
    public const string GpsDescriptionServices      = "Gps Notificaciones posMe";
    public const string GpsNameChangelNotification  = "GpsServicesChannel";
    public static string UrlRequestLogin            = UrlBase + "core_acount/loginMobile";
    public static string UrlRequestDownload         = UrlBase + "app_mobile_api/getDataDownload";
    public static string UrlUpload                  = UrlBase + "app_mobile_api/setDataUpload";
    public static string UrlSetUploadImageItem      = UrlBase + "app_mobile_api/setDataUploadImageItem";
    public static string UrlGetUploadImageItem      = UrlBase + "app_mobile_api/getDataUploadImageItem";
    public static string UrlLoadOrder               = UrlBase + "app_mobile_api/getDataLoadOrder";
    public const string UrlGPSShare                 = UrlBase + "app_mobile_api/setPositionGps";
    public const string UrlGpSShareOnly             = "app_mobile_api/setPositionGps";
    public const string UrlPagadito                 = "https://connect.pagadito.com/api/v2/exec-trans";
    public const string UrlPagaditoToken            = "https://comercios.pagadito.com/apipg/charges.php";
    public const string TokenPagadito               = "";
    public const string DescripcionRealizarPago     = "LICENCIA MOBIL";
    public const string fileError                   = "v4posme_error_log.txt";
    public const string fileBackupJson              = "v4posme_backup_log.txt";
    public const string ErrorUrl					= "Error";
    public const string UrlInvalida					= "URL inválida";
    public const string ParametrUrlShareViewError	= "No se pudo encontrar el parametro de la url";
    public const string CatalogName_MESAS                          = "CATALOGO DE MESAS";
    public const string InvoiceInMobileFieldCommentRequired        = "INVOICE_IN_MOBILE_FIELD_COMMENT_REQUERIED";
    public const string AppMobileSwitchCompany                     = "APP_MOBILE_SWITCH_COMPANY";
    // true = muestra el boton "Cargar solicitud" en la pantalla 2 de factura (requiere internet)
    public const string InvoiceMobileShowLoadOrder                 = "INVOICE_MOBILE_SHOW_LOAD_ORDER";
    // Rotulos de la pantalla 2 de factura; espejo de getBahavioDB labelCommentInvoiceV2Ext / labelZoneInvoiceV2Ext de la web
    public const string AppMobileLabelCommentInvoice               = "APP_MOBILE_LABEL_COMMENT_INVOICE";
    public const string AppMobileLabelZoneInvoice                  = "APP_MOBILE_LABEL_ZONE_INVOICE";
    // JSON que configura los campos de la factura móvil (visible, required, value_default, label)
    // y el título del documento. Lo interpreta HelperAppMobileSetting.
    public const string AppMobileSetting                           = "APP_MOBILE_SETTING";
    public const string SettingControlZona                         = "control_zone_facturacion";
    public const string SettingControlMesa                         = "control_mesa_facturacion";
    public const string SettingControlTipoDocumento                = "control_tipo_documento_facturacion";
    public const string SettingControlMoneda                       = "control_moneda_facturacion";
    public const string SettingControlTelefono                     = "control_telefono_facturacion";
    public const string SettingControlCedula                       = "control_cedula_facturacion";
    public const string SettingControlClienteReferencia            = "control_cliente_referencia_facturacion";
    public const string SettingLabelTypeDocumentInRegister         = "label_type_document_in_register";
    public const string SettingLabelTypeDocumentInAplied           = "label_type_document_in_aplied";
}