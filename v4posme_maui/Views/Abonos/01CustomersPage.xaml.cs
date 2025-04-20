using DevExpress.Maui.CollectionView;
using v4posme_maui.Models;
using v4posme_maui.ViewModels.Abonos;

namespace v4posme_maui.Views.Abonos;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AbonosPage : ContentPage
{
    private readonly AbonosViewModel _viewModel;

    public AbonosPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new AbonosViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }

    private void ClientesCollectionView_OnDropItem(object? sender, DropItemEventArgs e)
    {
        _viewModel.SavePositionCustomer(e);
    }
}