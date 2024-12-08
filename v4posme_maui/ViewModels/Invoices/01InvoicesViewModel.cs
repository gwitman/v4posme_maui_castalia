using System.Collections.ObjectModel;
using System.Windows.Input;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Invoices;

public class InvoicesViewModel : BaseViewModel
{
    private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;
	private readonly HelperCore _helper;
	public ICommand ItemTapped { get; }
    public ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers { get; }
    public ICommand SearchCommand { get; }
    public ICommand OnBarCode { get; }
    public Api_AppMobileApi_GetDataDownloadCustomerResponse? SelectedCustomer { get; set; }

    public InvoicesViewModel()
    {
        Title = "Selección de cliente 1/6";
        _customerRepositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
		_helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
		ItemTapped = new Command<Api_AppMobileApi_GetDataDownloadCustomerResponse>(OnItemTapped);
        SearchCommand = new Command(OnSearchCommand);
        OnBarCode = new Command(OnBarCodeShow);
        Customers = new();
    }

    private async void OnBarCodeShow()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar = await barCodePage.WaitForResultAsync();
        Search = bar!;
        if (string.IsNullOrWhiteSpace(Search)) return;
        OnSearchCommand(Search);
    }

    private async void OnItemTapped(Api_AppMobileApi_GetDataDownloadCustomerResponse? item)
    {
        if (item is null)
        {
            return;
        }

        IsBusy = true;
        VariablesGlobales.DtoInvoice = new ViewTempDtoInvoice
        {
            FirstName = item.FirstName,
            LastName = item.LastName,
            Balance = item.Balance
        };
        await NavigationService.NavigateToAsync<DataInvoicesViewModel>(item.CustomerNumber!);
        IsBusy = false;
    }

    private async void OnSearchCommand(object obj)
    {
        IsBusy = true;
        Customers.Clear();
        List<Api_AppMobileApi_GetDataDownloadCustomerResponse> finds;
        if (string.IsNullOrWhiteSpace(Search))
        {
			//para mostar un determinado numero de clientes
			var valueTop = await _helper.GetValueParameter("MOBILE_SHOW_TOP_CUSTOMER_IN_SHARE", "10");
			finds = await _customerRepositoryTbCustomer.PosMeAscTake10(int.Parse(valueTop));
        }
        else
        {
            finds = await _customerRepositoryTbCustomer.PosMeFilterByCustomerInvoice(Search);
        }

        foreach (var customer in finds)
        {
            Customers.Add(customer);
        }

        IsBusy = false;
    }

    public async void LoadsClientes()
    {
        Customers.Clear();
		//para mostar un determinado numero de clientes
		var valueTop = await _helper.GetValueParameter("MOBILE_SHOW_TOP_CUSTOMER_IN_SHARE", "10");
		var findAll = await _customerRepositoryTbCustomer.PosMeAscTake10(int.Parse(valueTop));
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