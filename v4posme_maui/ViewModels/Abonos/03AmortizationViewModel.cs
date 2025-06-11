using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Web;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using Unity;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.Abonos;

public class CreditDetailInvoiceViewModel : BaseViewModel, IQueryAttributable
{
    private string _transactionNumber = "";
    private bool _enableShareHistory;
    private Api_AppMobileApi_GetDataDownloadParametersResponse? _shareHistoryParam;
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;
    private readonly IRepositoryParameters _repositoryParameters;
    private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization;
    public ObservableCollection<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> Items { get; }
    public Command<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse> SwipeCommand { get; }
    public ICommand ViewPaymentsCommand { get; }

    public CreditDetailInvoiceViewModel()
    {
        Title                                   = "Selección de cuota 3/5";
        _repositoryDocumentCredit               = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        _repositoryParameters                   = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _repositoryDocumentCreditAmortization   = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
        Items               = new();
        SwipeCommand        = new Command<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>(OnSwipeCommand);
        ViewPaymentsCommand = new Command(OnViewPaymentsCommand);
    }

    public bool EnableShareHistory
    {
        get => _enableShareHistory;
        set => SetProperty(ref _enableShareHistory,value);
    }
    
    private async void OnViewPaymentsCommand()
    {
        try
        {
            _shareHistoryParam   = await _repositoryParameters.PosMeFindByKey("MOBILE_SHOW_URL_CUSTOMER_PAY");
            if (_shareHistoryParam is null || string.IsNullOrWhiteSpace(_shareHistoryParam.Value))
            {
                ShowToast(Constantes.ParametrUrlShareViewError, ToastDuration.Long, 14);
                return;
            }

            var baseUrl     = _shareHistoryParam.Value.Replace("{0}", _transactionNumber);
            var uriBuilder  = new UriBuilder(baseUrl);
            var query       = HttpUtility.ParseQueryString(uriBuilder.Query);
            /*
            query["q"]          = "MAUI .NET 8";
            query["hl"]         = "es";*/
            uriBuilder.Query    = query.ToString();
            if (Uri.TryCreate(uriBuilder.ToString(), UriKind.Absolute, out var uri))
            {
                await Launcher.Default.OpenAsync(uri);
            }
            else
            {
                // Manejo de URL no válida
                await Application.Current.MainPage.DisplayAlert(Constantes.ErrorUrl, Constantes.UrlInvalida, "OK");
            }
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
    }

    private async void OnSwipeCommand(Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse? item)
    {
        if (item is null)
        {
            return;
        }

        await NavigationService.NavigateToAsync<AplicarAbonoViewModel>(item.DocumentNumber!);
    }

    public override async Task InitializeAsync(object parameter)
    {
        await LoadInvoices(parameter as string);
    }

    private async Task LoadInvoices(string? parameter)
    {
        var find = await _repositoryDocumentCreditAmortization.PosMeFilterByDocumentNumber(parameter!);
        Items.Clear();
        foreach (var response in find)
        {
            Items.Add(response);
        }
        
        IsBusy = false;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        _transactionNumber = HttpUtility.UrlDecode(query["id"] as string);
        await LoadInvoices(_transactionNumber);
    }
}