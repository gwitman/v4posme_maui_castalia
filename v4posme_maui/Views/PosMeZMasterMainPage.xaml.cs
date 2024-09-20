using System.ComponentModel;
using System.Diagnostics;
using v4posme_maui.Services.Helpers;
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
        }
        
        void OnMenuItemClicked(object sender, EventArgs e)
        {
            VariablesGlobales.CompanyKey = string.Empty;
            Application.Current!.MainPage = new LoginPage();
        }
    }
}