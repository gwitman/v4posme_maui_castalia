using DevExpress.Maui.Scheduler;
using v4posme_maui.ViewModels;

namespace v4posme_maui.Views.Items
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemsPage : ContentPage
    {

        private PosMeItemsViewModel? _viewModel;
        public ItemsPage()
        {
            InitializeComponent();
            Title = "Productos";
            _viewModel = (PosMeItemsViewModel?)BindingContext;
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel!.OnAppearing(Navigation);
            _viewModel.LoadItems();
        }
    }
}