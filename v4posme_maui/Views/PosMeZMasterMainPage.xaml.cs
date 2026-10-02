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

            // Refrescar el encabezado cada vez que se abre el menu lateral, de modo que
            // tras descargar datos se muestre el comercio/usuario y el logo actualizados.
            PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(FlyoutIsPresented) && FlyoutIsPresented)
                {
                    LoadHeaderInfo();
                }
            };

            LoadHeaderInfo();
        }

        public async void LoadHeaderInfo()
        {
            try
            {
                // Nombre del comercio almacenado en tb_company (datos descargados),
                // concatenado con el usuario logueado: "Comercio / Usuario".
                var companyName = VariablesGlobales.TbCompany?.Name;
                var userName = VariablesGlobales.User?.Nickname;
                if (!string.IsNullOrWhiteSpace(companyName) && !string.IsNullOrWhiteSpace(userName))
                {
                    HeaderCompanyNameLabel.Text = $"{companyName} / {userName}";
                }
                else if (!string.IsNullOrWhiteSpace(companyName))
                {
                    HeaderCompanyNameLabel.Text = companyName;
                }
                else if (!string.IsNullOrWhiteSpace(userName))
                {
                    HeaderCompanyNameLabel.Text = userName;
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
