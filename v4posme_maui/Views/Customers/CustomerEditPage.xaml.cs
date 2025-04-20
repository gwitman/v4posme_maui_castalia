using DevExpress.Maui.Core;
using DevExpress.Maui.DataForm;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.Views.Customers;

public partial class CustomerEditPage : ContentPage
{
    private DetailEditFormViewModel ViewModel => (DetailEditFormViewModel)BindingContext;
    private static IRepositoryTbCustomer RepositoryTbCustomer => VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
    private readonly HelperCore _helperContador;
    private Api_AppMobileApi_GetDataDownloadCustomerResponse _saveItem;
    private Api_AppMobileApi_GetDataDownloadCustomerResponse _defaultItem;

    public CustomerEditPage()
    {
        InitializeComponent();
        _helperContador = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _saveItem = new Api_AppMobileApi_GetDataDownloadCustomerResponse();
        _defaultItem = new Api_AppMobileApi_GetDataDownloadCustomerResponse();
        Title = "Editar Cliente";
    }

    private async void BarCodeOnClicked(object? sender, EventArgs e)
    {
        var barCodePage = new BarCodePage();
        await Navigation.PushModalAsync(barCodePage);
        var bar = await barCodePage.WaitForResultAsync();
        if (string.IsNullOrWhiteSpace(bar)) return;
        TxtBarCode.Text = bar;
    }

    private async void SaveItemClick(object? sender, EventArgs e)
    {
        try
        {
            if (!DataForm.Validate())
            {
                TxtMensaje.Text = Mensajes.MensajeCampoRequerido;
                Popup.IsOpen    = true;
                return;
            }

            var saveCustomer               = (Api_AppMobileApi_GetDataDownloadCustomerResponse)DataForm.DataObject;
            saveCustomer.Modificado        = true;
            var validateCustomerNoExist = await RepositoryTbCustomer.PosMeExisteCustomerIdentification(saveCustomer.Identification!, saveCustomer.CustomerId);
            if (validateCustomerNoExist >= 1)
            {
                TxtMensaje.Text = $"{Mensajes.ExisteCustomerIdentificacion} {saveCustomer.Identification}";
                Popup.IsOpen    = true;
                return;
            }
            if (ViewModel.IsNew)
            {
                saveCustomer.CurrencyName = "Cordoba";
                saveCustomer.CurrencyId = (int)TypeCurrency.Cordoba;
                saveCustomer.CompanyId = Constantes.CompanyId;
                saveCustomer.BranchId = Constantes.BranchId;
                saveCustomer.EntityId = await _helperContador.GetAutoIncrement();
                saveCustomer.CustomerCreditLineId = await _helperContador.GetAutoIncrement();
                saveCustomer.CustomerNumber = (await _helperContador.GetAutoIncrement()).ToString();
                await RepositoryTbCustomer.PosMeInsert(saveCustomer);
            }
            else
            {
                await RepositoryTbCustomer.PosMeUpdate(saveCustomer);
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
        if (string.IsNullOrWhiteSpace(TxtBarCode.Text))
        {
            e.HasErrors = true;
            TxtBarCode.HasError = true;
        }

        if (string.IsNullOrWhiteSpace(TextCustomerNumber.Text))
        {
            e.HasErrors = true;
            TextCustomerNumber.HasError = true;
        }

        if (string.IsNullOrWhiteSpace(TextFirstName.Text))
        {
            e.HasErrors = true;
            TextFirstName.HasError = true;
        }

        if (string.IsNullOrWhiteSpace(TextLastName.Text))
        {
            e.HasErrors = true;
            TextLastName.HasError = true;
        }

        if (string.IsNullOrWhiteSpace(TextBalance.Text))
        {
            e.HasErrors = true;
            TextBalance.HasError = true;
        }
        if (string.IsNullOrWhiteSpace(TxtLocation.Text))
        {
            e.HasErrors = true;
            TxtLocation.HasError = true;
        }
        if (string.IsNullOrWhiteSpace(TxtPhone.Text))
        {
            e.HasErrors = true;
            TxtPhone.HasError = true;
        }
    }

    protected override async void OnAppearing()
    {
        if (ViewModel.IsNew) return;
        _saveItem = (Api_AppMobileApi_GetDataDownloadCustomerResponse)DataForm.DataObject;
        _defaultItem = await RepositoryTbCustomer.PosMeFindCustomer(_saveItem.CustomerNumber!);
        DataForm.CommitMode = CommitMode.LostFocus;
    }

    protected override void OnDisappearing()
    {
        if (!ViewModel.IsSaved)
        {
            TxtBarCode.Text = _defaultItem.Identification;
            TextCustomerNumber.Text = _defaultItem.CustomerNumber;
            TextFirstName.Text = _defaultItem.FirstName;
            TextLastName.Text = _defaultItem.LastName;
            TextBalance.Text = _defaultItem.Balance.ToString("N");
            DataForm.DataObject = _defaultItem;
        }
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}