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
        private readonly IRepositoryTbTransactionMasterDetail _transactionMasterDetail;
        


		public PosMeItemsViewModel()
        {
            IsBusy = true;
            _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
            _transactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
			Title = "Productos";
            _items = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>();
            CreateDetailFormViewModelCommand = new Command<CreateDetailFormViewModelEventArgs>(CreateDetailFormViewModel);
            SearchCommand = new Command(OnSearchItems);
            ChangeValueCommand = new Command(OnChangeValueCommand);
            OnBarCode = new Command(OnSearchBarCode);
        }


        public ICommand OnBarCode { get; }
        public ICommand SearchCommand { get; }
        public ICommand ChangeValueCommand { get;  }
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

        private void OnChangeValueCommand(object? obj)
        {
        }
        private void OnSearchItems(object? obj)
        {
            if (obj is not null)
            {
                Search = obj.ToString()!;
            }

            // Al reiniciar la lista, la posicion de navegacion tambien debe reiniciarse para
            // no arrastrar un indice que quedaria fuera de rango o apuntando a otro producto.
            VariablesGlobales.ItemsNavegacionIndex = 0;
            LoadItems();
        }

        private async void LoadItems()
        {
            IsBusy = true;
            await Task.Run(async () =>
            {
                // Se cargan TODOS los productos de una sola vez (sin paginacion ni take/top).
                List<Api_AppMobileApi_GetDataDownloadItemsResponse> allItems;
                if (string.IsNullOrWhiteSpace(Search))
                {
                    allItems = await _repositoryItems.PosMeAllOrderByName();
                }
                else
                {
                    allItems = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndNameAll(Search);
                }

                foreach (var item in allItems)
                {
                    var details = await _transactionMasterDetail.PosMeByTransactionIDAndItemID(
                        (int)TypeTransaction.TransactionInvoiceBilling, item.ItemId);
                    decimal cantidadFacturadas = details is null
                        ? 0
                        : Convert.ToDecimal(details.Where(p => p.RegisterLocal == 1).Sum(p => p.Quantity));
                    item.CantidadFacturadas = cantidadFacturadas;
                    item.CantidadFinal = (item.Quantity + item.CantidadEntradas) - (item.CantidadSalidas + cantidadFacturadas);
                }

                // Se reemplaza el contenido completo de la lista de una sola vez.
                Items.Clear();
                Items.AddRange(allItems);
                // Mantiene sincronizada la lista usada para navegar entre productos
                // (anterior/siguiente) desde las pantallas de detalle y edicion.
                VariablesGlobales.ItemsNavegacion = Items.ToList();
                IsBusy = false;
            });
        }
        
        
        private async void CreateDetailFormViewModel(CreateDetailFormViewModelEventArgs e)
        {
            if (e.DetailFormType != DetailFormType.Edit) return;
            var eItem = (Api_AppMobileApi_GetDataDownloadItemsResponse)e.Item;
            // PosMeFindByItemNumber es asincrono: hay que esperar el resultado para pasar el
            // producto (no la Task) al ViewModel de edicion, de lo contrario el formulario
            // recibe un objeto invalido y no muestra el registro.
            var item = await _repositoryItems.PosMeFindByItemNumber(eItem.ItemNumber!);
            e.Result = new DetailEditFormViewModel(item, isNew: false);
        }

        public void OnAppearing(INavigation navigation)
        {
            try
            {
                Navigation = navigation;
                LoadItems();
            }
            catch (Exception e)
            {
                HelperLogs.Log(e);
                ShowToast(e.Message, ToastDuration.Long, 14);
            }
        }
    }
}