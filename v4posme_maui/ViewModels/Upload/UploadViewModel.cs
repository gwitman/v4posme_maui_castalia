using System.Diagnostics;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.ViewModels.Upload;

public class UploadViewModel : BaseViewModel
{
    private readonly IRepositoryItems _repositoryItems;
    private readonly IRepositoryTbCustomer _repositoryTbCustomer;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail;
    private readonly HelperCore _helperContador;

    public UploadViewModel()
    {
        Title = "Subir Datos";
        _helperContador = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryTbTransactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
        UploadCommand = new Command(OnUploadCommand, ValidateUpload);
        PropertyChanged += (_, _) => UploadCommand.ChangeCanExecute();
    }

    private async void OnUploadCommand()
    {
        IsBusy = true;
        var counter = await _helperContador.GetCounter();
        if (counter == 0)
        {
            Mensaje = Mensajes.MensajeUploadCantidadTransacciones;
            PopupBackgroundColor = Colors.Red;
            PopUpShow = true;
            IsBusy = !IsBusy;
            return;
        }

        var api = new RestApiAppMobileApi();
        var response = await api.SendDataAsync();
        var apiResponse = JsonConvert.DeserializeObject<Api_AppMobileApi_SetDataUploadResponse>(response);
        if (apiResponse is not null)
        {
            if (apiResponse.Error)
            {
                Mensaje = $"{Mensajes.MensajeUploadError} {apiResponse.Message} ";
                PopupBackgroundColor = Colors.Red;
            }
            else
            {
                Mensaje = Mensajes.MensajeUploadSuccess;
                PopupBackgroundColor = Colors.Green;
                await _repositoryItems.PosMeDeleteAll();
                await _repositoryTbCustomer.PosMeDeleteAll();
                await _repositoryTbTransactionMaster.PosMeDeleteAll();
                await _repositoryTbTransactionMasterDetail.PosMeDeleteAll();
                await _helperContador.ZeroCounter();
            }

            PopUpShow = true;
            IsBusy = !IsBusy;
        }
        else
        {
            Mensaje = Mensajes.MensajeUploadError;
            PopupBackgroundColor = Colors.Red;
            PopUpShow = true;
            IsBusy = !IsBusy;
        }
    }

    private bool ValidateUpload()
    {
        return Connectivity.Current.NetworkAccess != NetworkAccess.None && Switch;
    }

    private async Task LoadData()
    {
        var findCustomers = await _repositoryTbCustomer.PosMeTakeModificados();
        var findItems = await _repositoryItems.PosMeTakeModificado();
        var findTransactionMaster = await _repositoryTbTransactionMaster.PosMeFindAll();
        var findTransactionMasterDetail = await _repositoryTbTransactionMasterDetail.PosMeFindAll();
    }

    private bool _switch;

    public bool Switch
    {
        get => _switch;
        set => SetProperty(ref _switch, value);
    }

    public Command UploadCommand { get; }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        IsBusy = false;
    }
}