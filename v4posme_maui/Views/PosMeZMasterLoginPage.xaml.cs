#nullable enable
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.ViewModels;
using v4posme_maui.Services.Repository;
using Unity;

namespace v4posme_maui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        public LoginPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            ((PosMeZMasterLoginViewModel)BindingContext).OnAppearing(Navigation);
        }

        private void ClosePopup_Clicked(object sender, EventArgs e)
        {
            Popup.IsOpen = false;
        }
    }
}