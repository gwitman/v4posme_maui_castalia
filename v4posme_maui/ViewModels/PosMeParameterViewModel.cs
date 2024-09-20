using System.Diagnostics;
using System.Windows.Input;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.ViewModels;

public class PosMeParameterViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _repositoryTbParameterSystem;
    private TbParameterSystem _posMeFindCounter = new();
    private TbParameterSystem _posMeFindLogo = new();    
    private TbParameterSystem _posmeFindPrinter = new();
    private TbParameterSystem _posmeFindCodigoAbono = new();
    private TbParameterSystem _posmeFindCodigFactura = new();
    public ICommand RefreshCommand { get; }
    public ICommand SaveCommand { get; }

    public PosMeParameterViewModel()
    {
        _repositoryTbParameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        Task.Run(async () =>
        {
            var test = await _repositoryTbParameterSystem.PosMeCount();
            Debug.WriteLine(test);
        });
        SaveCommand = new Command(OnSaveParameters);
        RefreshCommand = new Command(OnRefreshPage);
        LoadValuesDefault();
    }

    private void OnRefreshPage(object obj)
    {
        LoadValuesDefault();
        IsRefreshing = false;
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        LoadValuesDefault();
    }

    private async void LoadValuesDefault()
    {
        await Task.Run(async () =>
        {
            _posMeFindCounter = await _repositoryTbParameterSystem.PosMeFindCounter();
            if (!string.IsNullOrWhiteSpace(_posMeFindCounter.Value))
            {
                Contador = Convert.ToInt32(_posMeFindCounter.Value);
            }

            _posMeFindLogo = await _repositoryTbParameterSystem.PosMeFindLogo();
            if (!string.IsNullOrWhiteSpace(_posMeFindLogo.Value))
            {
                Logo = _posMeFindLogo.Value!;
                var imageBytes = Convert.FromBase64String(_posMeFindLogo.Value!);
                ShowImage = ImageSource.FromStream(() => new MemoryStream(imageBytes));
            }

            _posmeFindPrinter = await _repositoryTbParameterSystem.PosMeFindPrinter();
            if (!string.IsNullOrWhiteSpace(_posmeFindPrinter.Value))
            {
                Printer = _posmeFindPrinter.Value;
            }

            _posmeFindCodigoAbono = await _repositoryTbParameterSystem.PosMeFindCodigoAbono();
            if (!string.IsNullOrWhiteSpace(_posmeFindCodigoAbono.Value))
            {
                CodigoAbono = _posmeFindCodigoAbono.Value;
            }

            _posmeFindCodigFactura = await _repositoryTbParameterSystem.PosMeFindCodigoFactura();
            if (!string.IsNullOrWhiteSpace(_posmeFindCodigFactura.Value))
            {
                CodigoFactura = _posmeFindCodigFactura.Value;
            }

        });
    }

    private bool Validate()
    {
        
        PrinterHasError = string.IsNullOrWhiteSpace(Printer);
        AbonoHasError = string.IsNullOrWhiteSpace(CodigoAbono);
        FacturaHasError = string.IsNullOrWhiteSpace(CodigoFactura);
        return !( PrinterHasError || AbonoHasError || FacturaHasError);
    }

    private bool _printerhasError;

    public bool PrinterHasError
    {
        get => _printerhasError;
        set => SetProperty(ref _printerhasError, value);
    }


    private void OnSaveParameters(object obj)
    {
        try
        {
            if (Validate())
            {
                if (!string.IsNullOrWhiteSpace(VariablesGlobales.LogoTemp))
                {
                    _posMeFindLogo.Value = VariablesGlobales.LogoTemp;
                    _repositoryTbParameterSystem.PosMeUpdate(_posMeFindLogo);
                }

                _posMeFindCounter.Value = Contador.ToString();
                _repositoryTbParameterSystem.PosMeUpdate(_posMeFindCounter);                
                _posmeFindPrinter.Value = Printer;
                _repositoryTbParameterSystem.PosMeUpdate(_posmeFindPrinter);
                _posmeFindCodigFactura.Value = CodigoFactura;
                _repositoryTbParameterSystem.PosMeUpdate(_posmeFindCodigFactura);
                _posmeFindCodigoAbono.Value = CodigoAbono;
                _repositoryTbParameterSystem.PosMeUpdate(_posmeFindCodigoAbono);
                Mensaje = Mensajes.MensajeParametrosGuardar;
                PopupBackgroundColor = Colors.Green;
                LoadValuesDefault();
            }
            else
            {
                PopupBackgroundColor = Colors.Red;
                Mensaje = Mensajes.MensajeEspecificarDatosGuardar;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
            PopupBackgroundColor = Colors.Red;
            Mensaje = ex.Message;
        }

        PopUpShow = true;
    }

    private int _contador;

    public int Contador
    {
        get => _contador;
        set => SetProperty(ref _contador, value);
    }

    private string? _logo;

    public string? Logo
    {
        get => _logo;
        set => SetProperty(ref _logo, value);
    }
    

    private ImageSource? _showImage;

    public ImageSource? ShowImage
    {
        get => _showImage;
        set
        {
            _showImage = value;
            SetProperty(ref _showImage, value);
        }
    }

    private string? _printer;

    public string? Printer
    {
        get => _printer;
        set => SetProperty(ref _printer, value);
    }

    private bool _isRefreshing;

    public bool IsRefreshing
    {
        get => _isRefreshing;
        set => SetProperty(ref _isRefreshing, value);
    }

    private string? _codigoAbono;

    public string? CodigoAbono
    {
        get => _codigoAbono;
        set => SetProperty(ref _codigoAbono, value);
    }

    private bool _abonoHasError;

    public bool AbonoHasError
    {
        get => _abonoHasError;
        set => SetProperty(ref _abonoHasError, value);
    }

    private string? _codigoFactura;

    public string? CodigoFactura
    {
        get => _codigoFactura;
        set => SetProperty(ref _codigoFactura, value);
    }

    private bool _facturaHasError;

    public bool FacturaHasError
    {
        get => _facturaHasError;
        set => SetProperty(ref _facturaHasError, value);
    }
}