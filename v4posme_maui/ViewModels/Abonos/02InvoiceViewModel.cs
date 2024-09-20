using System.Collections.ObjectModel;
using System.Web;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using Unity;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;
namespace v4posme_maui.ViewModels.Abonos;

public class CustomerDetailInvoiceViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;
    private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization;

    public CustomerDetailInvoiceViewModel()
    {
        Title = "Selecciona Factura 2/5";
        _repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        _repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
        Invoices = new();
        SearchCommand = new Command(OnSearchCommand);
        ItemTapped = new Command<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>(OnTappedItem);
    }

    private async void OnTappedItem(Api_AppMobileApi_GetDataDownloadDocumentCreditResponse? item)
    {
        if (item is null)
        {
            return;
        }

        IsBusy = true;
        var count = await _repositoryDocumentCreditAmortization.PosMeCountByDocumentNumber(item.DocumentNumber!);
        if (count == 0)
        {
            ShowToast(Mensajes.MensajeDocumentCreditAmortizationVacio, ToastDuration.Short, 14);
            return;
        }

        await NavigationService.NavigateToAsync<CreditDetailInvoiceViewModel>(item.DocumentNumber!);
        IsBusy = false;
    }

    private async void OnSearchCommand(object obj)
    {
        IsBusy = true;
        var finder = await _repositoryDocumentCredit.PosMeFilterDocumentNumber(Search);
        Invoices.Clear();
        foreach (var item in finder)
        {
            Invoices.Add(item);
        }

        IsBusy = false;
    }

    public ObservableCollection<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> Invoices { get; }
    public ICommand SearchCommand { get; }

    private Api_AppMobileApi_GetDataDownloadDocumentCreditResponse? _selectedInvoice;

    public Api_AppMobileApi_GetDataDownloadDocumentCreditResponse? SelectedInvoice
    {
        get => _selectedInvoice;
        set
        {
            SetProperty(ref _selectedInvoice, value);
            OnPropertyChanged();
        }
    }

    public Command<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse> ItemTapped { get; }

    private async Task LoadInvoices(string? param)
    {
        Invoices.Clear();
        var invoicesEntityId = await _repositoryDocumentCredit.PosMeFindByEntityId(Convert.ToInt32(param));
        if (invoicesEntityId.Count == 0)
        {
            return;
        }

        foreach (var item in invoicesEntityId)
        {
            Invoices.Add(item);
        }

        IsBusy = false;
    }

    public override async Task InitializeAsync(object parameter)
    {
        await LoadInvoices(parameter as string);
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var id = HttpUtility.UrlDecode(query["id"] as string);
        await LoadInvoices(id);
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
    }
}