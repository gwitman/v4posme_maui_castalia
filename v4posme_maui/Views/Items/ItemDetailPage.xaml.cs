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

        // Ultimo producto que mostro esta pantalla de detalle. Sirve para detectar, al
        // reaparecer (por ejemplo al volver desde la edicion), si en la otra pantalla se
        // navego con Anterior/Siguiente a un producto distinto.
        private int _ultimoItemIdMostrado = -1;

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

            // Punto de partida: el producto que entrega DevExpress (el tocado en la lista).
            var itemIdAbrir = item.ItemId;
            var lista       = VariablesGlobales.ItemsNavegacion;

            if (lista is { Count: > 0 })
            {
                var indice = VariablesGlobales.ItemsNavegacionIndex;
                var itemIdEnIndice = (indice >= 0 && indice < lista.Count) ? lista[indice].ItemId : -1;

                // Determinar de forma fiable el producto a mostrar:
                // - Si el item de DevExpress cambio respecto a lo mostrado antes, se abrio un
                //   producto nuevo desde la lista: ese manda y sincronizamos el indice.
                // - Si no cambio pero el indice apunta a otro producto (navegacion en edicion),
                //   ese indice manda.
                if (item.ItemId != _ultimoItemIdMostrado)
                {
                    itemIdAbrir = item.ItemId;
                    var indiceActual = lista.FindIndex(p => p.ItemId == item.ItemId);
                    if (indiceActual >= 0)
                        VariablesGlobales.ItemsNavegacionIndex = indiceActual;
                }
                else if (itemIdEnIndice != -1)
                {
                    itemIdAbrir = itemIdEnIndice;
                }
            }

            var findItem                    = await _repositoryItems.PosMeFindByItemId(itemIdAbrir);
            SelectedItem                    = findItem;
            _ultimoItemIdMostrado           = itemIdAbrir;

            var objListTransactionDetail = await _transactionMasterDetail.PosMeByTransactionIDAndItemID((int)TypeTransaction.TransactionInvoiceBilling, itemIdAbrir);
            if (objListTransactionDetail is null)
                SelectedItem.CantidadFacturadas = 0;
            else
                SelectedItem.CantidadFacturadas = Convert.ToDecimal(objListTransactionDetail.Where(p => p.RegisterLocal == 1 ).Sum(p => p.Quantity));

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
                HelperLogs.Log(ex);
                _isDeleting = false;
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }

        private void CancelDeleteClick(object? sender, EventArgs e)
        {
            Popup.IsOpen = false;
        }

        private async void PreviousItemClick(object? sender, EventArgs e)
        {
            await NavegarItem(-1);
        }

        private async void NextItemClick(object? sender, EventArgs e)
        {
            await NavegarItem(1);
        }

        private async Task NavegarItem(int direccion)
        {
            try
            {
                var lista = VariablesGlobales.ItemsNavegacion;
                if (lista is null || lista.Count == 0)
                    return;

                var actual = (Api_AppMobileApi_GetDataDownloadItemsResponse)ViewModel.Item;
                var indiceActual = lista.FindIndex(p => p.ItemId == actual.ItemId);
                if (indiceActual < 0)
                    indiceActual = 0;

                var nuevoIndice = indiceActual + direccion;
                if (nuevoIndice < 0 || nuevoIndice >= lista.Count)
                    return;

                var siguiente = lista[nuevoIndice];
                var findItem  = await _repositoryItems.PosMeFindByItemId(siguiente.ItemId);
                SelectedItem  = findItem;

                var objListTransactionDetail = await _transactionMasterDetail.PosMeByTransactionIDAndItemID((int)TypeTransaction.TransactionInvoiceBilling, siguiente.ItemId);
                SelectedItem.CantidadFacturadas = objListTransactionDetail is null
                    ? 0
                    : Convert.ToDecimal(objListTransactionDetail.Where(p => p.RegisterLocal == 1).Sum(p => p.Quantity));

                SelectedItem.CantidadFinal = (SelectedItem.Quantity + SelectedItem.CantidadEntradas) - (SelectedItem.CantidadSalidas + SelectedItem.CantidadFacturadas);
                ViewModel.Item = SelectedItem;
                _ultimoItemIdMostrado = siguiente.ItemId;

                // Mantiene sincronizada la posicion de navegacion con la pantalla de edicion.
                VariablesGlobales.ItemsNavegacionIndex = nuevoIndice;
            }
            catch (Exception ex)
            {
                HelperLogs.Log(ex);
                await DisplayAlert("Error", ex.Message, "OK");
            }
        }
    }
}