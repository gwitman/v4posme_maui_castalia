using DevExpress.Maui.CollectionView;
using v4posme_maui.ViewModels;

namespace v4posme_maui.Views.Customers;

public partial class CustomersPage : ContentPage
{
    private readonly PosMeCustomerViewModel _clientesViewModel;

    public CustomersPage()
    {
        InitializeComponent();
        Title = "Clientes";
        _clientesViewModel = (PosMeCustomerViewModel)BindingContext;
    }

    protected override void OnAppearing()
    {
        _clientesViewModel.OnAppearing(Navigation);
    }

    private void ClientesCollectionView_OnDropItem(object? sender, DropItemEventArgs e)
    {
        _clientesViewModel.SavePositionCustomer(e);
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}