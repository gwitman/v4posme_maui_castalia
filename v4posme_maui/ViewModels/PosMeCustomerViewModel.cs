using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core.Internal;
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
    private readonly HelperCore _helper;
    private int _loadBatchSize = 15;
    private int _lastLoadedIndex;

    public PosMeCustomerViewModel()
    {
        _customerRepositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        Customers = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>();
        SearchCommand = new Command(OnSearchCommand);
        OnBarCode = new Command(OnBarCodeShow);
        LoadMoreCommand = new Command(OnLoadMoreCommand);
    }

    public ICommand OnBarCode { get; }
    public ICommand SearchCommand { get; }
    public ICommand LoadMoreCommand { get; }

    private DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> _customers = [];

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers
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

    private async void LoadCustomers()
    {
        IsBusy = true;
        await Task.Run(async () =>
        {
            Thread.Sleep(1000);
            List<Api_AppMobileApi_GetDataDownloadCustomerResponse> newCustomerResponses;
            if (string.IsNullOrWhiteSpace(Search))
            {
                newCustomerResponses = await _customerRepositoryTbCustomer.PosMeCustomerAscLoad(_lastLoadedIndex, _loadBatchSize);
            }
            else
            {
                newCustomerResponses = await _customerRepositoryTbCustomer.PosMeFilterBySearch(Search,_lastLoadedIndex, _loadBatchSize);
            }
            Customers.AddRange(newCustomerResponses);
        });
        IsBusy = false;
    }

    private void OnLoadMoreCommand()
    {
        LoadCustomers();
        _lastLoadedIndex += _loadBatchSize;
    }

    private void OnSearchCommand(object? obj)
    {
        IsBusy = true;
        if (obj is not null)
        {
            Search = obj.ToString()!;
        }
        Customers.Clear();
        _lastLoadedIndex = 0;
        LoadCustomers();
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

    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            Navigation = navigation;
            var topParameter = await _helper.GetValueParameter("MOBILE_SHOW_TOP_CUSTOMER", "10");
            _loadBatchSize = int.Parse(topParameter);
            _lastLoadedIndex = 0;
            Customers.Clear();
            LoadCustomers();
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 14);
        }
    }
}