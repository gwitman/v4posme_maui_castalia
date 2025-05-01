using v4posme_maui.Services.Helpers;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.SystemNames;
namespace v4posme_maui.ViewModels;

public class PosMeDownloadViewModel : BaseViewModel
{
    private bool _switch;
    private readonly RestApiAppMobileApi _restApiDownload;
    private readonly HelperCore _helperContador;

    public PosMeDownloadViewModel()
    {
        _helperContador = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _restApiDownload = new RestApiAppMobileApi();
        DownloadCommand = new Command(OnDownloadClicked, ValidateDownload);
        PropertyChanged += (_, _) => DownloadCommand.ChangeCanExecute();
    }

    private bool ValidateDownload()
    {
        return Connectivity.Current.NetworkAccess != NetworkAccess.None && Switch;
    }

    private async void OnDownloadClicked()
    {
        IsBusy = true;
        var counter = await _helperContador.GetCounter();
        if (counter != 0)
        {
            Mensaje = Mensajes.MensajeDownloadCantidadTransacciones;
            PopupBackgroundColor = Colors.Red;
            PopUpShow = true;
            IsBusy = !IsBusy;
            return;
        }

        var result = await _restApiDownload.GetDataDownload();
        if (result)
        {
            PopupBackgroundColor = Colors.Green;
            Mensaje = Mensajes.MensajeDownloadSuccess;
            VariablesGlobales.PermitirOrdenarFechaAbono = true;
        }
        else
        {
            Mensaje = Mensajes.MensajeDownloadError;
            PopupBackgroundColor = Colors.Red;
        }

        PopUpShow = true;
        IsBusy = false;
    }

    public bool Switch
    {
        get => _switch;
        set => SetProperty(ref _switch, value);
    }

    public Command DownloadCommand { get; }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        IsBusy = false;
    }
}