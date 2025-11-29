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
    public static readonly string ParametroAccesPoint               = "ACCESS_POINT";
    public static readonly string ParametroPrinter                  = "PRINTER";
    public static readonly string ParametroCodigoAbono              = "TRANSACTION_SHARE";
    public static readonly string ParemeterEntityIDAutoIncrement    = "AUTO_INCREMENT";
    public const string ParameterCodigoFactura                      = "TRANSACTION_INVOICE";
	public const string ParameterCodigoVisita                       = "TRANSACTION_VISIT";
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
    public const string CatalogName_MESAS           = "CATALOGO DE MESAS";
}