using System.Diagnostics;
using Unity;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
namespace v4posme_maui.Views
{
    public partial class MainPage : Shell
    {
        public MainPage()
        {
            InitializeComponent();
            Navigated += (sender, e) =>
            {
                var current = e.Current?.Location?.ToString();
                Debug.WriteLine($"Current tab: {current}");
            };

            LoadHeaderInfo();
        }

        private async void LoadHeaderInfo()
        {
            try
            {
                // Nombre del comercio almacenado en tb_company (datos descargados).
                var companyName = VariablesGlobales.TbCompany?.Name;
                if (!string.IsNullOrWhiteSpace(companyName))
                {
                    HeaderCompanyNameLabel.Text = companyName;
                }

                // Imagen configurada en la pagina de parametros (LOGO). Si no hay, se
                // mantiene la imagen por defecto (pm_png_96px.png).
                var repositoryParameter = VariablesGlobales.UnityContainer
                    .Resolve<IRepositoryTbParameterSystem>();
                var logo = await repositoryParameter.PosMeFindLogo();
                if (!string.IsNullOrWhiteSpace(logo?.Value))
                {
                    var imageBytes = Convert.FromBase64String(logo.Value!);
                    HeaderLogoImage.Source = ImageSource.FromStream(() => new MemoryStream(imageBytes));
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine($"Error cargando encabezado del menu: {e.Message}");
            }
        }

        void OnMenuItemClicked(object sender, EventArgs e)
        {
            VariablesGlobales.CompanyKey = string.Empty;
            Application.Current!.MainPage = new LoginPage();
        }
    }
}
