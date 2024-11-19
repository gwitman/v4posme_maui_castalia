using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using DevExpress.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;

namespace v4posme_maui.ViewModels
{
    public class PosMeItemsViewModel : BaseViewModel
    {
        private readonly IRepositoryItems _repositoryItems;
		private readonly HelperCore _helper;

		public PosMeItemsViewModel()
        {
            IsBusy = true;
            _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
			_helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
			Title = "Productos";
            _items = new ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>();
            CreateDetailFormViewModelCommand = new Command<CreateDetailFormViewModelEventArgs>(CreateDetailFormViewModel);
            SearchCommand = new Command(OnSearchItems);
            OnBarCode = new Command(OnSearchBarCode);
        }


        public ICommand OnBarCode { get; }
        public ICommand SearchCommand { get; }
        public ICommand CreateDetailFormViewModelCommand { get; }
        
        private ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> _items;

        public ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Items
        {
            get => _items;
            set => SetProperty(ref _items, value);
        }


        private Api_AppMobileApi_GetDataDownloadItemsResponse? _selectedItem;

        public Api_AppMobileApi_GetDataDownloadItemsResponse? SelectedItem
        {
            get => _selectedItem;
            set => SetProperty(ref _selectedItem, value);
        }

        private async void OnSearchBarCode(object obj)
        {
            var barCodePage = new BarCodePage();
            await Navigation!.PushModalAsync(barCodePage);
            var bar = await barCodePage.WaitForResultAsync();
            Search = bar!;
            OnSearchItems(Search);
        }

        private async void OnSearchItems(object? obj)
        {
            IsBusy = true;
            if (obj is not null)
            {
                Search = obj.ToString()!;
            }

            Items.Clear();
            var searchItems = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndName(Search);
            Items = new ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>(searchItems);

            IsBusy = false;
        }

        public async void LoadItems()
        {
            Items.Clear();
            await Task.Run(async () =>
            {
                Thread.Sleep(1000);
				//para mostar un determinado numero de clientes
				var valueTop = await _helper.GetValueParameter("MOBILE_SHOW_TOP_ITEMS", "10");

				var newItems = await _repositoryItems.PosMeDescending10(int.Parse(valueTop));
                Items = new ObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>(newItems);
            });
            IsBusy = false;
        }

        private void CreateDetailFormViewModel(CreateDetailFormViewModelEventArgs e)
        {
            if (e.DetailFormType != DetailFormType.Edit) return;
            var eItem = (Api_AppMobileApi_GetDataDownloadItemsResponse)e.Item;
            var item = _repositoryItems.PosMeFindByItemNumber(eItem.ItemNumber!);
            e.Result = new DetailEditFormViewModel(item, isNew: false);
        }

        public void OnAppearing(INavigation navigation)
        {
            Navigation = navigation;
        }
    }
}