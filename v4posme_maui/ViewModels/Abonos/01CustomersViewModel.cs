using System.Diagnostics;
using System.Windows.Input;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Scheduler;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using AndroidX.Lifecycle;

namespace v4posme_maui.ViewModels.Abonos;

public class AbonosViewModel : BaseViewModel
{
    private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;
    private readonly IRepositoryTbParameterSystem _repositoryTbParameterSystem;
    private readonly HelperCore _helperCore;

    public AbonosViewModel()
    {
        Title = "Selección de cliente 1/5";
        _customerRepositoryTbCustomer   = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryDocumentCredit       = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        _repositoryTbParameterSystem    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _helperCore                     = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        _customers                      = new();
        SearchCommand                   = new Command(OnSearchCommand);
        OnBarCode                       = new Command(OnBarCodeShow);
        ItemTapped                      = new Command<Api_AppMobileApi_GetDataDownloadCustomerResponse>(OnItemSelected);
    }

    private async void OnBarCodeShow()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar         = await barCodePage.WaitForResultAsync();
        Search          = bar!;
        if (string.IsNullOrWhiteSpace(Search)) return;
        OnSearchCommand();
    }

    private async void OnItemSelected(Api_AppMobileApi_GetDataDownloadCustomerResponse? item)
    {
        if (item is null)
        {
            return;
        }

        IsBusy          = true;
        var invoices    = await _repositoryDocumentCredit.PosMeFindByEntityId(item.EntityId);
        if (invoices.Count == 0)
        {
            ShowToast(Mensajes.MensajeDocumentCreditCustomerVacio, ToastDuration.Short, 14);
            return;
        }

        await NavigationService.NavigateToAsync<CustomerDetailInvoiceViewModel>(item.EntityId);
        IsBusy = false;
    }

    private async void OnSearchCommand()
    {
        try
        {
            IsBusy = true;
            LoadsClientes();
            IsBusy = false;
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
    }

    public ICommand SearchCommand { get; }

    private DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> _customers;

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers
    {
        get => _customers;
        set => SetProperty(ref _customers, value);
    }

    private Api_AppMobileApi_GetDataDownloadCustomerResponse? _selectedCustomer;

    public Api_AppMobileApi_GetDataDownloadCustomerResponse? SelectedCustomer
    {
        get => _selectedCustomer;
        set
        {
            SetProperty(ref _selectedCustomer, value);
            OnItemSelected(value!);
            OnPropertyChanged();
        }
    }

    public Command<Api_AppMobileApi_GetDataDownloadCustomerResponse> ItemTapped { get; }
    public ICommand OnBarCode { get; }

    private async void LoadsClientes()
    {
        try
        {
            List<Api_AppMobileApi_GetDataDownloadCustomerResponse> allCustomers;
            IsBusy                  = true;
            if(VariablesGlobales.OrdenarAbonos )
            {
                // 1. Obtener todos los clientes                
                if (string.IsNullOrWhiteSpace(Search))
                {
                    allCustomers = await _customerRepositoryTbCustomer.PosMeFilterByShare();
                }
                else
                {
                    allCustomers = await _customerRepositoryTbCustomer.PosMeFilterByCustomerShare(Search);
                }

                //2. Ordenar
                allCustomers = await _helperCore.ReordenarListaAbono(allCustomers);

                //Lenar el elemento vacio
                if (string.IsNullOrWhiteSpace(Search))
                {
                    var empty = new Api_AppMobileApi_GetDataDownloadCustomerResponse();
                    allCustomers.Add(empty);
                }else{}

            }
            else
            {
                // 1. Obtener todos los clientes
                if (string.IsNullOrWhiteSpace(Search))
                {
                    allCustomers    = await _customerRepositoryTbCustomer.PosMeFilterByShare();

                    //Lenar el elemento vacio
                    var empty       = new Api_AppMobileApi_GetDataDownloadCustomerResponse();
                    allCustomers.Add(empty);
                }
                else
                {
                    allCustomers = await _customerRepositoryTbCustomer.PosMeFilterByCustomerShare(Search);
                }
            }

            
            //8. Agregar a la lista principal
            Customers = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(allCustomers);
            


        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading customers: {ex.Message}");
            ShowToast(ex.Message, ToastDuration.Long, 14);
        }
        finally
        {
            IsBusy = false;
        }
    }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        LoadsClientes();
        
    }

    public async void SavePositionCustomer(DropItemEventArgs e)
    {
        try
        {
            IsBusy = true;
            if (e.DragItem is not Api_AppMobileApi_GetDataDownloadCustomerResponse customer || e.DropItemHandle < 0)
            {
                return;
            }

            var oldCustomer         = (Api_AppMobileApi_GetDataDownloadCustomerResponse)e.DropItem;
            var oldCustomerNumber   = oldCustomer.CustomerNumber;
            var oldEntityId         = oldCustomer.EntityId;
            var oldPosition         = e.ItemHandle;
            var newEntityID         = customer.EntityId;
            var newPosition         = e.DropItemHandle;
            var newCustomerNumber   = customer.CustomerNumber;

            //Obtener parametro de configuracion
            var parameter           = await _repositoryTbParameterSystem.PosMeFindCustomerOrderShare();
            var currentPositions    = new List<CustomerOrderShare>();
            var customerList        = await _customerRepositoryTbCustomer.PosMeFindAll();

            //Obtener la posicion actual
            var customerItem        = customerList.Where(p => p.EntityId == newEntityID).FirstOrDefault();
            var positionActual      = customerItem is null ? 0 : customerItem.SecuenciaAbono;

            //Desplazar posiciones de los item que no se tocaron
            if (positionActual > newPosition)
            {   
                foreach (var cus in customerList.Where(p => p.SecuenciaAbono >= newPosition).ToList())
                {
                    cus.SecuenciaAbono++;
                }
            }
            else
            {
                foreach (var cus in customerList.Where(p => p.SecuenciaAbono <= newPosition).ToList())
                {
                    cus.SecuenciaAbono--;
                }
            }

            //Desplazamiento de posiciones del item que se toco
            if(customerItem is not null)
            customerItem.SecuenciaAbono = newPosition;


            //Crear el nuevo array con sus posciciones
            foreach(var cus in customerList.OrderBy(p => p.SecuenciaAbono))
            {
                currentPositions.Add(new CustomerOrderShare
                {
                    EntityId        = cus.EntityId,
                    Position        = cus.SecuenciaAbono,
                    customerNumber  = cus.CustomerNumber is null ? "" : cus.CustomerNumber
                });
            }


            //Actualizar las posiciones en las tablas
            parameter.Value                 = JsonConvert.SerializeObject(currentPositions);
            await _customerRepositoryTbCustomer.PosMeUpdateAll(customerList);
            await _repositoryTbParameterSystem.PosMeUpdate(parameter);
            LoadsClientes();
            
       
        }
        catch (Exception ex)
        {
            ShowToast("Error al guardar la posición: " + ex.Message, ToastDuration.Long, 14);
        }
    }
}