using System.Collections.ObjectModel;
using System.Web;
using System.Windows.Input;
using Android.Media;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.DataGrid;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;
using Unity;
using v4posme_maui.ViewModels;
namespace v4posme_maui.ViewModels.Invoices;

public class DataInvoiceCreditViewModel : BaseViewModel, IQueryAttributable
{

    private IRepositoryTbCustomer _repositoryTbCustomer;

    public DataInvoiceCreditViewModel()
    {
        _repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        Title = "Datos de credito 3/6";
        Item = VariablesGlobales.DtoInvoice;
        SeleccionarProductosCommand = new Command(OnSeleccionarProductos);
        PropertyChanged += (_, _) => SeleccionarProductosCommand.ChangeCanExecute();
        LoadComboBox();
    }

    private async void OnSeleccionarProductos()
    {        
        IsBusy = true;
        if (SelectedPeriodPay is null)
        {
            ShowToast(Mensajes.MensajeSeleccionarFrecuenciaPago, ToastDuration.Long, 16);
            IsBusy = false;
            return;
        }

        Item.NextVisit      = NextVisit;
        Item.Plazo          = Plazo;
        Item.FixedExpenses  = FixedExpenses;
        Item.PeriodPay      = SelectedPeriodPay;
        // Facturacion rapida: se regresa mediante pop a la pantalla de seleccion de
        // producto (4/6) conservando los productos y sin apilar una nueva instancia.
        await NavigationService.GoBackAsync();
        IsBusy              = false;
    }




    //Comando siguiente
    public Command SeleccionarProductosCommand { get; }

    //Factura
    public ViewTempDtoInvoice Item { get; private set; }


    //Periodo de pago
    private ObservableCollection<DtoCatalogItem>? _PeriodPay;

    public ObservableCollection<DtoCatalogItem>? PeriodPay
    {
        get => _PeriodPay;
        set => SetProperty(ref _PeriodPay, value);
    }
    public DtoCatalogItem? SelectedPeriodPay { get; set; }

    public bool ErrorPeriodPay { get; set; }

    //Interes
    public bool ErrorFiexdExpense { get; set; }

    private decimal _FixedExpenses = 0;
    public decimal FixedExpenses
    {
        get => _FixedExpenses;
        set => SetProperty(ref _FixedExpenses, value);
    }

    //Plazo
    public bool ErrorPlazo { get; set; }

    private int _Plazo = 1;

    public int Plazo
    {
        get => _Plazo;
        set => SetProperty(ref _Plazo, value);
    }

    //Primer pago
    public bool ErrorNextVisit { get; set; }

    private DateTime _NextVisit = DateTime.Now.Date;
    public DateTime NextVisit
    {
        get => _NextVisit;
        set => SetProperty(ref _NextVisit, value);
    }


    public void OnAppearing(INavigation? navigation)
    {
        Navigation = navigation;
    }

    public async void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        var id = HttpUtility.UrlDecode(query["id"] as string);
        await LoadData(id);
    }

    private async Task LoadData(string? id)
    {
        var customer    = await _repositoryTbCustomer.PosMeFindCustomer(id!);
        Item            = VariablesGlobales.DtoInvoice;
        VariablesGlobales.DtoInvoice.CustomerResponse = customer;

        // Respetar los valores ya definidos en el DTO (por ejemplo, al volver desde el
        // menu desplegable de la pantalla 4/6). Solo se aplican los valores por defecto
        // cuando aun no existen datos previos.
        NextVisit       = Item.NextVisit == default ? DateTime.Now.Date : Item.NextVisit;
        FixedExpenses   = Item.FixedExpenses;
        Plazo           = Item.Plazo <= 0 ? 1 : Item.Plazo;

        LoadComboBox();
        IsBusy = false;
    }

    private void LoadComboBox()
    {
         
        PeriodPay =
        [
            new DtoCatalogItem((int)TypePeriodPay.Mensual, "Mensual", "M"),
            new DtoCatalogItem((int)TypePeriodPay.Quincenal, "Quincenal", "Q"),
            new DtoCatalogItem((int)TypePeriodPay.Semanal, "Semanal", "S"),
            new DtoCatalogItem((int)TypePeriodPay.Diario, "Diario", "D"),
        ];
        if (PeriodPay.Any())
        {
            SelectedPeriodPay = PeriodPay.First();
        }

        if (VariablesGlobales.DtoInvoice.PeriodPay != null)
        {
            SelectedPeriodPay = PeriodPay.Where(k => k.Key == VariablesGlobales.DtoInvoice.PeriodPay.Key).FirstOrDefault();
        }

    }

}
