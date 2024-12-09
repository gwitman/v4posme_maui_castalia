using v4posme_maui.Services;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Views;
using v4posme_maui.Views.Abonos;
using v4posme_maui.Views.Invoices;
using v4posme_maui.Views.Items;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views.Printers;
using Application = Microsoft.Maui.Controls.Application;
using Android.Content;
using v4posme_maui.Views.More;
using v4posme_maui.Views.More.ReporteVenta;
using v4posme_maui.Views.More.Visita;

namespace v4posme_maui
{
    public partial class App : Application
    {
        private readonly IServiceProvider _services;

        public App(IServiceProvider services)
        {
            _services = services;
            InitializeComponent();
            var dataBase = new DataBase();
            dataBase.Init();
            dataBase.InitDownloadTables();
            MainPage = new LoginPage();
            UserAppTheme = AppTheme.Light;
		}


        protected override void OnStart()
        {
            DependencyService.Register<NavigationService>();
            Routing.RegisterRoute(typeof(ItemDetailPage).FullName, typeof(ItemDetailPage));
            //Routing.RegisterRoute(typeof(AboutPage).FullName, typeof(AboutPage));
            Routing.RegisterRoute(typeof(CustomerDetailInvoicePage).FullName, typeof(CustomerDetailInvoicePage));
            Routing.RegisterRoute(typeof(AbonosPage).FullName, typeof(AbonosPage));
            Routing.RegisterRoute(typeof(CreditDetailInvoicePage).FullName, typeof(CreditDetailInvoicePage));
            Routing.RegisterRoute(typeof(AplicarAbonoPage).FullName, typeof(AplicarAbonoPage));
            Routing.RegisterRoute(typeof(ValidarAbonoPage).FullName, typeof(ValidarAbonoPage));
			Routing.RegisterRoute(typeof(ValidarAbonoHideSaldoPage).FullName, typeof(ValidarAbonoHideSaldoPage));
			Routing.RegisterRoute(typeof(DataInvoicesPage).FullName, typeof(DataInvoicesPage));
            Routing.RegisterRoute(typeof(DataInvoiceCreditPage).FullName, typeof(DataInvoiceCreditPage));
            Routing.RegisterRoute(typeof(SeleccionarProductoPage).FullName, typeof(SeleccionarProductoPage));
            Routing.RegisterRoute(typeof(PaymentInvoicePage).FullName, typeof(PaymentInvoicePage));
            Routing.RegisterRoute(typeof(RevisarProductosSeleccionadosPage).FullName, typeof(RevisarProductosSeleccionadosPage));
            Routing.RegisterRoute(typeof(VoucherInvoicePage).FullName, typeof(VoucherInvoicePage));
            Routing.RegisterRoute(typeof(MorePage).FullName, typeof(MorePage));
			Routing.RegisterRoute(typeof(ReporteVentaPage).FullName, typeof(ReporteVentaPage));
			Routing.RegisterRoute(typeof(VisitaPage).FullName, typeof(VisitaPage));
		}

        public static void StartGpsService()
        {
            var intent = new Intent(Android.App.Application.Context, typeof(GPSService));
            Android.App.Application.Context.StartService(intent);
        }
    }
}