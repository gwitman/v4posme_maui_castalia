using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.SystemNames;


namespace v4posme_maui.ViewModels.More.Visita;
internal class VisitaViewModel : BaseViewModel
{
    private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;

    public VisitaViewModel()
    {
        Title = "Selección de cliente 1/2";
        _customerRepositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        Customers = new();
        SearchCommand = new Command(OnSearchCommand);
        OnBarCode = new Command(OnBarCodeShow);
        ItemTapped = new Command<Api_AppMobileApi_GetDataDownloadCustomerResponse>(OnItemSelected);
    }

    private async void OnBarCodeShow()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar = await barCodePage.WaitForResultAsync();
        Search = bar!;
        if (string.IsNullOrWhiteSpace(Search)) return;
        OnSearchCommand();
    }

    private async void OnItemSelected(Api_AppMobileApi_GetDataDownloadCustomerResponse? item)
    {
        if (item is null)
        {
            return;
        }

        IsBusy = true;
        var invoices = await _repositoryDocumentCredit.PosMeFindByEntityId(item.EntityId);
        if (invoices.Count == 0)
        {
            ShowToast(Mensajes.MensajeDocumentCreditCustomerVacio, ToastDuration.Short, 14);
            return;
        }

        await NavigationService.NavigateToAsync<VisitaFormViewModel>(item.EntityId);
        IsBusy = false;
    }

    private async void OnSearchCommand()
    {
        IsBusy = true;
        List<Api_AppMobileApi_GetDataDownloadCustomerResponse> finds;
        if (string.IsNullOrWhiteSpace(Search))
        {
            finds = await _customerRepositoryTbCustomer.PosMeFilterByInvoice();
        }
        else
        {
            finds = await _customerRepositoryTbCustomer.PosMeFilterByCustomerInvoice(Search);
        }

        Customers.Clear();
        foreach (var customer in finds)
        {
            Customers.Add(customer);
        }

        IsBusy = false;
    }

    public ICommand SearchCommand { get; }
    public ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers { get; }

    private Api_AppMobileApi_GetDataDownloadCustomerResponse? _selectedCustomer;

    public Api_AppMobileApi_GetDataDownloadCustomerResponse? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            SetProperty(ref _selectedCustomer, value);
            OnItemSelected(value!);
            OnPropertyChanged();
        }
    }

    public Command<Api_AppMobileApi_GetDataDownloadCustomerResponse> ItemTapped { get; }
    public ICommand OnBarCode { get; }

    public async Task LoadsClientes()
    {
        Customers.Clear();
        var findAll = await _customerRepositoryTbCustomer.PosMeFilterByInvoice();
        foreach (var response in findAll)
        {
            Customers.Add(response);
        }

        IsBusy = false;
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
    }
}

