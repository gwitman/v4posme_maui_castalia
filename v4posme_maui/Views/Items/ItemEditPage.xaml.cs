using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core;
using DevExpress.Maui.DataForm;
using v4posme_maui.Models;
using v4posme_maui.Services;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;
using v4posme_maui.ViewModels;
using Android.Icu.Text;

namespace v4posme_maui.Views.Items;

public partial class ItemEditPage : ContentPage
{
    private DetailEditFormViewModel ViewModel => (DetailEditFormViewModel)BindingContext;
    private readonly IRepositoryItems _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
    private readonly IRepositoryTbTransactionMasterDetail _transactionMasterDetail;
    private Api_AppMobileApi_GetDataDownloadItemsResponse _saveItem;
    private Api_AppMobileApi_GetDataDownloadItemsResponse _defaultItem;
    private readonly HelperCore _helperContador;
    public ItemEditPage()
    {
        InitializeComponent();
        _saveItem = new Api_AppMobileApi_GetDataDownloadItemsResponse();
        _defaultItem = new Api_AppMobileApi_GetDataDownloadItemsResponse();
        _helperContador = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _transactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        DataForm.CommitMode = CommitMode.Manually;
        Title = "Editar Producto";
    }

    private async void SaveItemClick(object sender, EventArgs e)
    {
        try
        {
            if (!DataForm.Validate())
            {
                TxtMensaje.Text = Mensajes.MensajeCampoRequerido;
                Popup.IsOpen    = true;
                return;
            }

            //Validar Permiso
            bool permission = await _helperContador.GetPermission(TypeMenuElementID.app_inventory_item_index_aspx, TypePermission.Updated, TypeImpact.All);
            if (!permission)
            {
                TxtMensaje.Text = Mensajes.MensajeNoTienePermisoDeEdicion;
                Popup.IsOpen    = true;
                return;
            }

            _saveItem            = (Api_AppMobileApi_GetDataDownloadItemsResponse)DataForm.DataObject;
            _saveItem.Modificado = true;
            var count            = await _repositoryItems.PosMeExistBarCode(_saveItem.BarCode, _saveItem.ItemId);
            if (count >= 1)
            {
                TxtMensaje.Text = $"{Mensajes.ExisteItem} {_saveItem.BarCode}";
                Popup.IsOpen    = true;
                return;
            }
            if (ViewModel.IsNew)
            {
                await _repositoryItems.PosMeInsert(_saveItem);
            }
            else
            {
                await _repositoryItems.PosMeUpdate(_saveItem);
            }

            await _helperContador.PlusCounter();
            DataForm.Commit();
            ViewModel.Save();
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
            TxtMensaje.Text = ex.Message;
            Popup.IsOpen    = true;
            return;
        }
    }

    private void DataForm_OnValidateForm(object sender, DataFormValidationEventArgs e)
    {
        _saveItem = (Api_AppMobileApi_GetDataDownloadItemsResponse)e.DataObject;
        if (string.IsNullOrWhiteSpace(_saveItem.ItemNumber))
        {
            e.HasErrors = true;
            TextItemNumber.HasError = true;
        }
        else
        {
            TextItemNumber.HasError = false;
        }

        if (string.IsNullOrWhiteSpace(_saveItem.BarCode))
        {
            e.HasErrors = true;
            TxtBarCode.HasError = true;
        }
        else
        {
            TxtBarCode.HasError = false;
        }

        if (string.IsNullOrWhiteSpace(_saveItem.Name))
        {
            e.HasErrors = true;
            TextName.HasError = true;
        }
        else
        {
            TextName.HasError = false;
        }

        if (string.IsNullOrWhiteSpace(TextPrecioPublico.Text))
        {
            e.HasErrors = true;
            TextPrecioPublico.HasError = true;
        }
        else
        {
            TextPrecioPublico.HasError = false;
        }
    }

    private async void SimpleButton_OnClicked(object? sender, EventArgs e)
    {
        var barCodePage = new BarCodePage();
        await Navigation.PushModalAsync(barCodePage, true);
        var bar = await barCodePage.WaitForResultAsync();
        if (string.IsNullOrWhiteSpace(bar)) return;
        TxtBarCode.Text = bar;
    }

    private void TextCantidadEntrada_OnTextChanged(object? sender, EventArgs e)
    {
        _saveItem.CantidadFinal = decimal.Add(_saveItem.CantidadEntradas, _saveItem.Quantity) - (_saveItem.CantidadSalidas + _saveItem.CantidadFacturadas);        
    }

    protected override async void OnAppearing()
    {
        if (!ViewModel.IsNew)
        {
            // La edicion se abre desde el detalle (boton Editar). DevExpress entrega en
            // DataObject su referencia interna, que corresponde al producto con el que se
            // ABRIO el detalle, NO al producto al que se navego con Anterior/Siguiente.
            // Por eso la fuente de verdad del producto vigente es ItemsNavegacionIndex, que
            // el detalle mantiene actualizado al navegar.
            var selected = (Api_AppMobileApi_GetDataDownloadItemsResponse)DataForm.DataObject;
            var itemIdAbrir = selected.ItemId;

            var lista = VariablesGlobales.ItemsNavegacion;
            if (lista is { Count: > 0 })
            {
                var indice = VariablesGlobales.ItemsNavegacionIndex;
                if (indice >= 0 && indice < lista.Count)
                    itemIdAbrir = lista[indice].ItemId;
            }

            // Recargamos los datos frescos del producto vigente (por su ItemId) para reflejar
            // cualquier cambio guardado y calcular las cantidades derivadas.
            var itemFresco = await _repositoryItems.PosMeFindByItemId(itemIdAbrir);

            var objListTransactionDetail = await _transactionMasterDetail.PosMeByTransactionIDAndItemID((int)TypeTransaction.TransactionInvoiceBilling, itemFresco.ItemId);
            var quatityInvoice = objListTransactionDetail is null
                ? 0
                : Convert.ToDecimal(objListTransactionDetail.Where(p => p.RegisterLocal == 1).Sum(p => p.Quantity));

            itemFresco.CantidadFacturadas = quatityInvoice;
            itemFresco.CantidadFinal      = (itemFresco.Quantity + itemFresco.CantidadEntradas) - (itemFresco.CantidadSalidas + quatityInvoice);

            _saveItem           = itemFresco;
            _defaultItem        = itemFresco;
            DataForm.DataObject = itemFresco;
            // El indice de navegacion ya apunta al producto vigente (se leyo arriba),
            // por lo que no es necesario recalcularlo aqui.
        }

        DataForm.CommitMode = CommitMode.LostFocus;
    }

    protected override void OnDisappearing()
    {
        if (ViewModel.IsSaved) return;


        TxtBarCode.Text             = _defaultItem.BarCode;
        TextCantidadFinal.Text      = _defaultItem.CantidadFinal.ToString("N2");
        TextCantidadEntrada.Text    = _defaultItem.CantidadEntradas.ToString("N2");
        TextCantidadSalida.Text     = _defaultItem.CantidadSalidas.ToString("N2");
        TextName.Text               = _defaultItem.Name;
        TextItemNumber.Text         = _defaultItem.ItemNumber;
        TextCantidadFacturadas.Text = _defaultItem.CantidadFacturadas.ToString("N2");
        TextPrecioPublico.Text      = _defaultItem.PrecioPublico.ToString("N2");
        TextCosto.Text              = _defaultItem.Cost.ToString("N2");
        DataForm.DataObject         = _defaultItem;
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}