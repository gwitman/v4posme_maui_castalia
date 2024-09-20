using DevExpress.Maui.Core;
using DevExpress.Maui.DataForm;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;
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
        if (!DataForm.Validate())
            return;

        var saveCustomer = (Api_AppMobileApi_GetDataDownloadCustomerResponse)DataForm.DataObject;
        saveCustomer.Modificado = true;
        if (ViewModel.IsNew)
        {
            saveCustomer.CustomerCreditLineId = 0;
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
}