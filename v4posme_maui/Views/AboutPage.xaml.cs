using v4posme_maui.ViewModels;

namespace v4posme_maui.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ((AboutViewModel)BindingContext).OnAppearing(Navigation);
        }

        private async void ComboCompanias_SelectionChanged(object? sender, EventArgs e)
        {
            if (ComboCompanias.SelectedItem is not string nuevaCompania)
            {
                return;
            }

            await ((AboutViewModel)BindingContext).OnCompaniaSeleccionadaCambio(nuevaCompania);
        }
    }
}