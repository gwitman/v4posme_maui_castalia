using DevExpress.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;
using v4posme_maui.Services.HelpersPrinters.Epson_Commands;

namespace v4posme_maui.Views.Items
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ItemDetailPage : ContentPage
    {
        private DetailFormViewModel ViewModel => ((DetailFormViewModel)BindingContext);
        private readonly IRepositoryItems _repositoryItems;
        private bool _isDeleting;        
        private readonly IRepositoryTbTransactionMasterDetail _transactionMasterDetail;

        private Api_AppMobileApi_GetDataDownloadItemsResponse SelectedItem { get; set; }

        public ItemDetailPage()
        {
            Title                       = "Datos de Producto";
            _repositoryItems            = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
            _transactionMasterDetail    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
            SelectedItem                = new();
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var item                        = (Api_AppMobileApi_GetDataDownloadItemsResponse)ViewModel.Item;
            var findItem                    = await _repositoryItems.PosMeFindByItemId(item.ItemId);
            SelectedItem                    = findItem;

            var objListTransactionDetail = await _transactionMasterDetail.PosMeByTransactionIDAndItemID((int)TypeTransaction.TransactionInvoiceBilling, item.ItemId);
            if (objListTransactionDetail is null)
                SelectedItem.CantidadFacturadas = 0;
            else
                SelectedItem.CantidadFacturadas = Convert.ToDecimal(objListTransactionDetail.Sum(p => p.Quantity));

            SelectedItem.CantidadFinal  = (SelectedItem.Quantity +  SelectedItem.CantidadEntradas) - (SelectedItem.CantidadSalidas + SelectedItem.CantidadFacturadas);
            ViewModel.Item              = SelectedItem;
            
        }

        private void DeleteItemClick(object? sender, EventArgs e)
        {
            Popup.IsOpen = true;
        }

        private async void DeleteConfirmedClick(object? sender, EventArgs e)
        {
            if (_isDeleting)
                return;
            _isDeleting = true;

            try
            {
                var helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
                _isDeleting = await _repositoryItems.PosMeDelete(SelectedItem);
                if (_isDeleting)
                {
                    await helper.PlusCounter();
                }

                ViewModel.Close();
            }
            catch (Exception ex)
            {
                _isDeleting = false;
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void CancelDeleteClick(object? sender, EventArgs e)
        {
            Popup.IsOpen = false;
        }
    }
}