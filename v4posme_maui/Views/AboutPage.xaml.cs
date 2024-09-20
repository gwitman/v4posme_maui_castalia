using v4posme_maui.ViewModels;
using SelectionChangedEventArgs = DevExpress.Maui.Charts.SelectionChangedEventArgs;

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

        private void ChartView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            
        }
    }
}