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

namespace v4posme_maui.Views.Items;

public partial class ItemEditPage : ContentPage
{
    private DetailEditFormViewModel ViewModel => (DetailEditFormViewModel)BindingContext;
    private readonly IRepositoryItems _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
    private Api_AppMobileApi_GetDataDownloadItemsResponse _saveItem;
    private Api_AppMobileApi_GetDataDownloadItemsResponse _defaultItem;
    private readonly HelperCore _helperContador;
    public ItemEditPage()
    {
        InitializeComponent();
        _saveItem = new Api_AppMobileApi_GetDataDownloadItemsResponse();
        _defaultItem = new Api_AppMobileApi_GetDataDownloadItemsResponse();
        _helperContador = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
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
            if ( !permission)
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
        _saveItem.CantidadFinal = decimal.Add(_saveItem.CantidadEntradas, _saveItem.Quantity) - _saveItem.CantidadSalidas;
        TextCantidadFinal.Text = _saveItem.CantidadFinal.ToString("N2");
    }

    protected override async void OnAppearing()
    {
        if (!ViewModel.IsNew)
        {
            _saveItem = (Api_AppMobileApi_GetDataDownloadItemsResponse)DataForm.DataObject;
            _defaultItem = await _repositoryItems.PosMeFindByItemNumber(_saveItem.ItemNumber!);
        }

        DataForm.CommitMode = CommitMode.LostFocus;
    }

    protected override void OnDisappearing()
    {
        if (ViewModel.IsSaved) return;
        TxtBarCode.Text = _defaultItem.BarCode;
        TextCantidadFinal.Text = _defaultItem.CantidadFinal.ToString("N2");
        TextCantidadEntrada.Text = _defaultItem.CantidadEntradas.ToString("N2");
        TextCantidadSalida.Text = _defaultItem.CantidadSalidas.ToString("N2");
        TextName.Text = _defaultItem.Name;
        TextItemNumber.Text = _defaultItem.ItemNumber;
        TextPrecioPublico.Text = _defaultItem.PrecioPublico.ToString("N2");
        DataForm.DataObject = _defaultItem;
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}