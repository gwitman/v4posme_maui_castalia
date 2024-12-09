using System.Collections.ObjectModel;
using System.Windows.Input;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;

namespace v4posme_maui.ViewModels.More.Visita
{
	public class VisitaViewModel : PosMeCustomerViewModel
	{
		/*private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;
		private readonly HelperCore _helper;		
		private ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> _customers = [];
		private Api_AppMobileApi_GetDataDownloadCustomerResponse? _selectedCustomer;

		public ObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers
		{
			get => _customers;
			set => SetProperty(ref _customers, value);
		}

		public ICommand OnBarCode { get; }
		public ICommand SearchCommand { get; }
		
		public Api_AppMobileApi_GetDataDownloadCustomerResponse? SelectedCustomer
		{
			get => _selectedCustomer;
			set => SetProperty(ref _selectedCustomer, value);
		}

		public VisitaViewModel()
		{
			_customerRepositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
			_helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
			Customers = [];
			SearchCommand = new Command(OnSearchCommand);
			OnBarCode = new Command(OnBarCodeShow);
		}

		private async void OnSearchCommand(object obj)
		{
			IsBusy = true;
			List<Api_AppMobileApi_GetDataDownloadCustomerResponse> finds;
			if (string.IsNullOrWhiteSpace(Search))
			{
				//para mostar un determinado numero de clientes
				var valueTop = await _helper.GetValueParameter("MOBILE_SHOW_TOP_CUSTOMER", "10");

				finds = await _customerRepositoryTbCustomer.PosMeAscTake10(int.Parse(valueTop));
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
		}*/
	}
}
