using System.ComponentModel;
using System.Runtime.CompilerServices;
using CommunityToolkit.Maui.Core;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Helpers;

// Encapsula el flujo reutilizable del boton "Subir Datos": sube los datos locales al
// servidor y, si la subida es exitosa, descarga la informacion actualizada. El flujo es
// el mismo que el de la ventana 8 de la factura (08PrinterInvoiceViewModel.OnSubirCommand)
// y esta gobernado por el parametro de servidor MOBILE_UPLOAD_AFTER_INVOICE.
//
// Expone el estado de los dos popups (confirmacion y resultado) y el indicador de espera
// para que cualquier pantalla (compra, ajuste, entrada/salida de inventario, gasto,
// salida/ingreso de efectivo, etc.) pueda enlazarlos sin duplicar la logica.
//
// Uso en un ViewModel:
//   public HelperUploadDownload Sync { get; } = new();
// En la pagina (XAML) se enlaza el boton a Sync.MostrarConfirmacionSubidaCommand y los
// dos DXPopup a las propiedades Sync.*. En el code-behind se cablean los Clicked a los
// metodos Confirmar/Cancelar/CerrarResultado.
public class HelperUploadDownload : INotifyPropertyChanged
{
    private readonly HelperCore _helperCore;
    private readonly IRepositoryItems _repositoryItems;
    private readonly IRepositoryTbCustomer _repositoryTbCustomer;
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail;
    private readonly RestApiAppMobileApi _restApi = new();

    // Iconos (simbolos unicode) usados en el popup de resultado.
    private const string IconoExito = "\u2713"; // check
    private const string IconoError = "\u2715"; // x
    private const string IconoAviso = "!";

    public HelperUploadDownload()
    {
        _helperCore                          = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _repositoryItems                     = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _repositoryTbCustomer                = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryTbTransactionMaster       = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
        _repositoryTbTransactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();

        MostrarConfirmacionSubidaCommand = new Command(() => MostrarConfirmacionSubida = true);
        SubirCommand                     = new Command(OnSubirCommand);
    }

    // Abre el popup de confirmacion (lo invoca el boton "Subir Datos").
    public Command MostrarConfirmacionSubidaCommand { get; }

    // Ejecuta la subida/descarga (lo invoca el boton "Subir" del popup de confirmacion).
    public Command SubirCommand { get; }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    private string _loadingMessage = string.Empty;
    public string LoadingMessage
    {
        get => _loadingMessage;
        private set => SetProperty(ref _loadingMessage, value);
    }

    private bool _mostrarConfirmacionSubida;
    public bool MostrarConfirmacionSubida
    {
        get => _mostrarConfirmacionSubida;
        set => SetProperty(ref _mostrarConfirmacionSubida, value);
    }

    private bool _mostrarResultado;
    public bool MostrarResultado
    {
        get => _mostrarResultado;
        set => SetProperty(ref _mostrarResultado, value);
    }

    private string _resultadoTitulo = string.Empty;
    public string ResultadoTitulo
    {
        get => _resultadoTitulo;
        private set => SetProperty(ref _resultadoTitulo, value);
    }

    private string _resultadoMensaje = string.Empty;
    public string ResultadoMensaje
    {
        get => _resultadoMensaje;
        private set => SetProperty(ref _resultadoMensaje, value);
    }

    private string _resultadoIcono = string.Empty;
    public string ResultadoIcono
    {
        get => _resultadoIcono;
        private set => SetProperty(ref _resultadoIcono, value);
    }

    private Color _resultadoColor = Colors.Green;
    public Color ResultadoColor
    {
        get => _resultadoColor;
        private set
        {
            if (SetProperty(ref _resultadoColor, value))
                OnPropertyChanged(nameof(ResultadoColorSuave));
        }
    }

    // Version clara del color de acento para el fondo del circulo del icono.
    public Color ResultadoColorSuave => _resultadoColor.WithAlpha(0.15f);

    // Cierra el popup de confirmacion sin hacer nada (boton "Cancelar").
    public void CancelarConfirmacion()
    {
        MostrarConfirmacionSubida = false;
    }

    // Cierra el popup de confirmacion y dispara la subida (boton "Subir").
    public void ConfirmarSubida()
    {
        MostrarConfirmacionSubida = false;
        SubirCommand.Execute(null);
    }

    // Cierra el popup de resultado (boton "Aceptar").
    public void CerrarResultado()
    {
        MostrarResultado = false;
    }

    private async void OnSubirCommand()
    {
        // Evitar reentrada: si ya hay una operacion en curso, ignorar nuevos toques.
        if (IsBusy)
            return;

        var uploadAfterInvoiceValue = await _helperCore.GetValueParameter("MOBILE_UPLOAD_AFTER_INVOICE", "false");
        var uploadAfterInvoice      = bool.TryParse(uploadAfterInvoiceValue, out var parsedValue) && parsedValue;
        if (uploadAfterInvoice)
        {
            IsBusy         = true;
            LoadingMessage = Mensajes.MensajeSubiendoDatos;

            // Subir datos
            var response    = await _restApi.SendDataAsync();
            var apiResponse = Newtonsoft.Json.JsonConvert.DeserializeObject<Api_AppMobileApi_SetDataUploadResponse>(response);
            if (apiResponse is not null)
            {
                if (apiResponse.Error)
                {
                    // La subida fallo: NO se descarga.
                    MostrarResultadoPopup(Mensajes.TituloUploadError, $"{Mensajes.MensajeUploadError} {apiResponse.Message}", Colors.Red, IconoError);
                }
                else
                {
                    // Subida exitosa: limpiar local, reiniciar contador y recien
                    // entonces proceder con la descarga.
                    await _repositoryItems.PosMeDeleteAll();
                    await _repositoryTbCustomer.PosMeDeleteAll();
                    await _repositoryTbTransactionMaster.PosMeDeleteAll();
                    await _repositoryTbTransactionMasterDetail.PosMeDeleteAll();
                    await _helperCore.ZeroCounter();

                    // Descargar datos (solo si la subida fue exitosa)
                    LoadingMessage = Mensajes.MensajeDescargandoDatos;
                    var counter = await _helperCore.GetCounter();
                    if (counter != 0)
                        await _restApi.GetDataDownload(true);
                    else
                        await _restApi.GetDataDownload(false);

                    MostrarResultadoPopup(Mensajes.TituloUploadSuccess, Mensajes.MensajeUploadSuccess, Colors.Green, IconoExito);
                }
            }
            else
            {
                // Respuesta nula: la subida fallo, NO se descarga.
                MostrarResultadoPopup(Mensajes.TituloUploadError, Mensajes.MensajeUploadError, Colors.Red, IconoError);
            }

            LoadingMessage = string.Empty;
            IsBusy = false;
        }
        else
        {
            // Parametro desactivado o inexistente: no se sube nada automaticamente.
            MostrarResultadoPopup(Mensajes.TituloUploadNoAutomatico, Mensajes.MensajeUploadNoAutomatico, Color.FromArgb("#E67E22"), IconoAviso);
        }
    }

    // Muestra el popup de resultado con estilo de marca.
    private void MostrarResultadoPopup(string titulo, string mensaje, Color color, string icono)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ResultadoTitulo  = titulo;
            ResultadoMensaje = mensaje;
            ResultadoColor   = color;
            ResultadoIcono   = icono;
            MostrarResultado = true;
        });
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;
        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    private void OnPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
