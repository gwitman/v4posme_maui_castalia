using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core;
using DevExpress.Maui.Core.Internal;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels
{
    public class PosMeItemsViewModel : BaseViewModel
    {
        private readonly IRepositoryItems _repositoryItems;
		private readonly HelperCore _helper;
        private int _loadBatchSize = 15;
        private int _lastLoadedIndex;


		public PosMeItemsViewModel()
        {
            IsBusy = true;
            _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
			_helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
			Title = "Productos";
            _items = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>();
            CreateDetailFormViewModelCommand = new Command<CreateDetailFormViewModelEventArgs>(CreateDetailFormViewModel);
            SearchCommand = new Command(OnSearchItems);
            OnBarCode = new Command(OnSearchBarCode);
            LoadMoreCommand = new Command(OnLoadMoreCommand);
        }


        public ICommand OnBarCode { get; }
        public ICommand SearchCommand { get; }
        public ICommand LoadMoreCommand { get; }
        public ICommand CreateDetailFormViewModelCommand { get; }
        
        private DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> _items;

        public DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Items
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

        private void OnSearchItems(object? obj)
        {
            IsBusy = true;
            if (obj is not null)
            {
                Search = obj.ToString()!;
            }
            
            _lastLoadedIndex = 0;
            Items.Clear();
            LoadItems();
            IsBusy = false;
        }

        private async void LoadItems()
        {
            IsBusy = true;
            await Task.Run(async () =>
            {
                Thread.Sleep(1000);
                List<Api_AppMobileApi_GetDataDownloadItemsResponse> newItems;
                if (string.IsNullOrWhiteSpace(Search))
                { 
                    newItems = await _repositoryItems.PosMeDescendingBySizeAndTop(_lastLoadedIndex, _loadBatchSize);
                }
                else
                {
                    newItems = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndNameByTop(Search, _lastLoadedIndex, _loadBatchSize);
                }
				
                Items.AddRange(newItems);
            });
            IsBusy = false;
        }
        
        private void OnLoadMoreCommand()
        {
            LoadItems();
            _lastLoadedIndex += _loadBatchSize;
        }
        private void CreateDetailFormViewModel(CreateDetailFormViewModelEventArgs e)
        {
            if (e.DetailFormType != DetailFormType.Edit) return;
            var eItem = (Api_AppMobileApi_GetDataDownloadItemsResponse)e.Item;
            var item = _repositoryItems.PosMeFindByItemNumber(eItem.ItemNumber!);
            e.Result = new DetailEditFormViewModel(item, isNew: false);
        }

        public async void OnAppearing(INavigation navigation)
        {
            try
            {
                Navigation = navigation;
                var topParameter = await _helper.GetValueParameter("MOBILE_SHOW_TOP_CUSTOMER", "10");
                _loadBatchSize = int.Parse(topParameter);
                _lastLoadedIndex = 0;
                Items.Clear();
                LoadItems();
            }
            catch (Exception e)
            {
                ShowToast(e.Message, ToastDuration.Long, 14);
            }
        }
    }
}