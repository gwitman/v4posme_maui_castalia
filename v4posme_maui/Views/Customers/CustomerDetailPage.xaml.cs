using DevExpress.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using Unity;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;
namespace v4posme_maui.Views.Customers;

public partial class CustomerDetailPage : ContentPage
{
    private readonly IRepositoryTbCustomer _repositoryTbCustomer;
    private DetailFormViewModel ViewModel => (DetailFormViewModel)BindingContext;
    private Api_AppMobileApi_GetDataDownloadCustomerResponse SelectedItem => (Api_AppMobileApi_GetDataDownloadCustomerResponse)ViewModel.Item;
    private bool _isDeleting;

    public CustomerDetailPage()
    {
        InitializeComponent();
        Title = "Datos de Cliente";
        _repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
    }

    private void DeleteItemClick(object? sender, EventArgs e)
    {
        Popup.IsOpen = true;
    }

    private async void DeleteConfirmedClick(object? sender, EventArgs e)
    {
        if (_isDeleting)
            return;
        _isDeleting = true;

        try
        {
            var helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
            _isDeleting = await _repositoryTbCustomer.PosMeDelete(SelectedItem);
            if (_isDeleting)
            {
                await helper.PlusCounter();
            }
            ViewModel.Close();
        }
        catch (Exception ex)
        {
            _isDeleting = false;
            await DisplayAlert("Error", ex.Message, "OK");
        }
    }

    private void CancelDeleteClick(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}