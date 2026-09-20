using System.Diagnostics;
using System.Windows.Input;
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Core.Internal;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels;

public class PosMeCustomerViewModel : BaseViewModel
{
    private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;
    private readonly IRepositoryTbParameterSystem _repositoryTbParameterSystem;
    private readonly HelperCore _helperCore;
    

    public PosMeCustomerViewModel()
    {
        _customerRepositoryTbCustomer   = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryTbParameterSystem    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _helperCore                     = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        Customers                       = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>();
        SearchCommand                   = new Command(OnSearchCommand);
        OnBarCode                       = new Command(OnBarCodeShow);
    }

    public ICommand OnBarCode { get; }
    public ICommand SearchCommand { get; }

    private DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> _customers = [];

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers
    {
        get => _customers;
        set => SetProperty(ref _customers, value);
    }

    private Api_AppMobileApi_GetDataDownloadCustomerResponse? _selectedCustomer;

    public Api_AppMobileApi_GetDataDownloadCustomerResponse? SelectedCustomer
    {
        get => _selectedCustomer;
        set => SetProperty(ref _selectedCustomer, value);
    }

    private void OnSearchCommand(object? obj)
    {
        if (obj is not null)
        {
            Search = obj.ToString() ?? string.Empty;
        }
        _ = LoadCustomers();
    }

    private async void OnBarCodeShow(object obj)
    {
        try
        {
            var barCodePage     = new BarCodePage();
            await Navigation!.PushModalAsync(barCodePage);
            var bar             = await barCodePage.WaitForResultAsync();
            Search              = bar!;
            if (string.IsNullOrWhiteSpace(Search)) return;
            OnSearchCommand(Search);
        }
        catch (Exception e)
        {
            HelperLogs.Log(e);
            ShowMensajePopUp(e.Message);
        }
    }

    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            Navigation           = navigation;
            await LoadCustomers();
        }
        catch (Exception e)
        {
            HelperLogs.Log(e);
            ShowMensajePopUp(e.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<bool> LoadCustomers()
    {
        try
        {
            IsBusy = true;

            // Si esta activo el reordenamiento personalizado, se aplica antes de leer.
            if (VariablesGlobales.OrdenarClientes)
            {
                await _helperCore.ReordenarListaClientes();
            }

            // Se cargan TODOS los clientes de una sola vez (sin paginacion ni take/limit).
            List<Api_AppMobileApi_GetDataDownloadCustomerResponse> allCustomers;
            if (string.IsNullOrWhiteSpace(Search))
            {
                allCustomers = await _customerRepositoryTbCustomer.PosMeCustomerAscLoadAll();
            }
            else
            {
                allCustomers = await _customerRepositoryTbCustomer.PosMeFilterBySearchAll(Search);
            }

            // Se reemplaza el contenido completo de la lista de una sola vez.
            Customers = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(allCustomers);
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
            Debug.WriteLine($"Error loading customers: {ex.Message}");
            ShowMensajePopUp(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }

        return true;
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

            // Obtener la lista actual de posiciones
            var parameter           = await _repositoryTbParameterSystem.PosMeFindCustomerOrderCustomer();
            var currentPositions    = new List<CustomerOrderShare>();
            var customerList        = await _customerRepositoryTbCustomer.PosMeFindAll();



            //Obtener la posicion actual
            var customerItem    = customerList.Where(p => p.EntityId == newEntityID).FirstOrDefault();
            var positionActual  = customerItem is null ? 0 : customerItem.Secuencia;

            //Desplazar posiciones de los item que no se tocaron
            if (positionActual > newPosition)
            {
                foreach (var cus in customerList.Where(p => p.Secuencia >= newPosition).ToList())
                {
                    cus.Secuencia++;
                }
            }
            else
            {
                foreach (var cus in customerList.Where(p => p.Secuencia <= newPosition).ToList())
                {
                    cus.Secuencia--;
                }
            }

            //Desplazamiento de posiciones del item que se toco
            if (customerItem is not null)
            customerItem.Secuencia = newPosition;


            //Crear el nuevo array con sus posciciones
            foreach (var cus in customerList.OrderBy(p => p.Secuencia))
            {
                currentPositions.Add(new CustomerOrderShare
                {
                    EntityId = cus.EntityId,
                    Position = cus.Secuencia is null ? 0 : cus?.Secuencia??0,
                    customerNumber = cus?.CustomerNumber is null ? "" : cus.CustomerNumber
                });
            }


            //Actualizar las posiciones en las tablas
            parameter.Value = JsonConvert.SerializeObject(currentPositions);
            await _customerRepositoryTbCustomer.PosMeUpdateAll(customerList);
            await _repositoryTbParameterSystem.PosMeUpdate(parameter);
            
        }
        catch (Exception ex)
        {
            HelperLogs.Log(ex);
            Debug.WriteLine(ex.StackTrace);
            ShowMensajePopUp(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}