using CommunityToolkit.Maui;
using DevExpress.Maui;
using DevExpress.Maui.Charts;
using DevExpress.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.ViewModels;
using v4posme_maui.Views;
using v4posme_maui.Views.Items;
using System.Text;
using SkiaSharp.Views.Maui.Controls.Hosting;
using Unity;
using ZXing.Net.Maui.Controls;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.SystemNames;


namespace v4posme_maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            ThemeManager.ApplyThemeToSystemBars = false;
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseDevExpress(useLocalization: true)
                .UseDevExpressDataGrid()
                .UseDevExpressCollectionView()
                .UseDevExpressEditors()
                .UseDevExpressScheduler()
                .UseDevExpressCharts()
                .UseDevExpressControls()
                .UseSkiaSharp()
                .UseMauiCommunityToolkit()
                .UseBarcodeReader()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("roboto-regular.ttf", "Roboto");
                    fonts.AddFont("roboto-medium.ttf", "Roboto-Medium");
                    fonts.AddFont("roboto-bold.ttf", "Roboto-Bold");
                    fonts.AddFont("univia-pro-regular.ttf", "Univia-Pro");
                    fonts.AddFont("univia-pro-medium.ttf", "Univia-Pro Medium");
                });

            VariablesGlobales.UnityContainer.RegisterType<IRepositoryTbUser, RepositoryTbUser>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryTbCustomer, RepositoryTbCustomer>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryTbCompany, RepositoryTbCompany>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryTbMenuElement, RepositoryTbMenuElement>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryDocumentCreditAmortization, RepositoryDocumentCreditAmortization>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryDocumentCredit, RepositoryDocumentCredit>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryItems, RepositoryItems>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryParameters, RepositoryParameters>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryTbParameterSystem, RepositoryTbParameterSystem>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryTbTransactionMaster, RepositoryTbTransactionMaster>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryTbTransactionMasterDetail, RepositoryTbTransactionMasterDetail>();
            VariablesGlobales.UnityContainer.RegisterType<IRepositoryServerTransactionMaster, RepositoryServerTransactionMaster>();
            VariablesGlobales.UnityContainer.RegisterSingleton<DataBase>();
            VariablesGlobales.UnityContainer.RegisterSingleton<HelperCore>();
            VariablesGlobales.UnityContainer.RegisterSingleton<DownloadPage>();
            VariablesGlobales.UnityContainer.RegisterSingleton<ItemDetailPage>();
            VariablesGlobales.UnityContainer.RegisterSingleton<ItemsPage>();
            VariablesGlobales.UnityContainer.RegisterSingleton<PosMeItemsViewModel>();
            Initializer.Init();
            DevExpress.Maui.CollectionView.Initializer.Init();
            DevExpress.Maui.Controls.Initializer.Init();
            DevExpress.Maui.Editors.Initializer.Init();
            DevExpress.Maui.DataGrid.Initializer.Init();
            DevExpress.Maui.Scheduler.Initializer.Init();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return builder.Build();

        }


    }
}
