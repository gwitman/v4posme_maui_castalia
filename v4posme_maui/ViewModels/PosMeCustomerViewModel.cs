using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;

namespace v4posme_maui.ViewModels;

public class PosMeCustomerViewModel : BaseViewModel
{
    private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;

    public PosMeCustomerViewModel()
    {
        _customerRepositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        Customers = new ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>();
        SearchCommand = new Command(OnSearchCommand);
        OnBarCode = new Command(OnBarCodeShow);
    }

    public ICommand OnBarCode { get; }
    public ICommand SearchCommand { get; }

    private ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> _customers = [];

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers
    {
        get => _customers;
        set => SetProperty(ref _customers, value);
    }

    private Api_AppMobileApi_GetDataDownloadCustomerResponse? _selectedCustomer;

    public Api_AppMobileApi_GetDataDownloadCustomerResponse? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    private async void OnSearchCommand(object obj)
    {
        IsBusy = true;
        List<Api_AppMobileApi_GetDataDownloadCustomerResponse> finds;
        if (string.IsNullOrWhiteSpace(Search))
        {
            finds = await _customerRepositoryTbCustomer.PosMeAscTake10();
        }
        else
        {
            finds = await _customerRepositoryTbCustomer.PosMeFilterBySearch(Search);
        }
        Customers.Clear();
        Customers = new ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(finds);
        IsBusy = false;
    }

    private async void OnBarCodeShow(object obj)
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar = await barCodePage.WaitForResultAsync();
        Search = bar!;
        if (string.IsNullOrWhiteSpace(Search)) return;
        OnSearchCommand(Search);
    }

    private async void LoadsClientes()
    {
        await Task.Run(async () =>
        {
            Thread.Sleep(1000);
            var findAll = await _customerRepositoryTbCustomer.PosMeAscTake10();
            Customers = new ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(findAll);
        });
        IsBusy = false;
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        LoadsClientes();
    }
}