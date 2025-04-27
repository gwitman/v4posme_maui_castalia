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
    private int _loadBatchSize = 10;
    private int _lastLoadedIndex;
    private List<CustomerOrderShare> _customerOrderShares = new();
    
    public PosMeCustomerViewModel()
    {
        _customerRepositoryTbCustomer   = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryTbParameterSystem    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _helperCore                         = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        Customers                       = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>();
        SearchCommand                   = new Command(OnSearchCommand);
        OnBarCode                       = new Command(OnBarCodeShow);
        LoadMoreCommand                 = new Command(OnLoadMoreCommand);
    }

    public ICommand OnBarCode { get; }
    public ICommand SearchCommand { get; }
    public ICommand LoadMoreCommand { get; }

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

    private void OnLoadMoreCommand()
    {
        LoadCustomers();
        _lastLoadedIndex += _loadBatchSize;
    }

    private void OnSearchCommand(object? obj)
    {
        IsBusy = true;
        if (obj is not null)
        {
            Search = obj.ToString() ?? string.Empty;
        }
        _lastLoadedIndex = 0;
        OnLoadMoreCommand();
        IsBusy = false;
    }

    private async void OnBarCodeShow(object obj)
    {
        try
        {
            var barCodePage = new BarCodePage();
            await Navigation!.PushModalAsync(barCodePage);
            var bar = await barCodePage.WaitForResultAsync();
            Search = bar!;
            if (string.IsNullOrWhiteSpace(Search)) return;
            OnSearchCommand(Search);
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
    }

    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            Navigation           = navigation;
            _lastLoadedIndex     = 0;
            var topParameter     = await _helperCore.GetValueParameter("MOBILE_SHOW_TOP_CUSTOMER", "10");
            _loadBatchSize       = int.Parse(topParameter);
            _customerOrderShares = await LoadOrderCustomer();
            OnLoadMoreCommand();
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<List<CustomerOrderShare>> LoadOrderCustomer()
    {
        var customerOrderJson = await _repositoryTbParameterSystem.PosMeFindCustomerOrderCustomer();
        List<CustomerOrderShare> customOrder = [];

        if (!string.IsNullOrWhiteSpace(customerOrderJson.Value))
        {
            customOrder = JsonConvert.DeserializeObject<List<CustomerOrderShare>>(customerOrderJson.Value) ?? [];
        }
        return customOrder;
    }
    
    private async void LoadCustomers()
    {
        try
        {
            IsBusy = true;
            // 1. Obtener el orden personalizado desde el repositorio
            var customOrder = _customerOrderShares;
            
            // 2. Obtener todos los clientes
            List<Api_AppMobileApi_GetDataDownloadCustomerResponse> allCustomers;
            if (_lastLoadedIndex == 0)
            {
                Customers.Clear();
            }
            if (string.IsNullOrWhiteSpace(Search))
            {
                allCustomers = await _customerRepositoryTbCustomer.PosMeCustomerAscLoad(_lastLoadedIndex, _loadBatchSize);
            }
            else
            {
                allCustomers = await _customerRepositoryTbCustomer.PosMeFilterBySearch(Search,_lastLoadedIndex, _loadBatchSize);
            }

            // 3. Si no hay orden personalizado, cargar directamente
            if (customOrder.Count == 0)
            {
                Customers.AddRange(allCustomers);
                IsBusy = false;
                return;
            }

            //5. Inicializar lista de clientes ordenados
            var clientesOrdenados = new List<Api_AppMobileApi_GetDataDownloadCustomerResponse>();

            //6. Colocar los personalizados
            foreach (var personalizado in customOrder.OrderBy(c => c.Position))
            {
                var cliente = allCustomers.FirstOrDefault(c => c.EntityId == personalizado.EntityId);
                if (cliente == null) continue;
                cliente.Secuencia = personalizado.Position;
                clientesOrdenados.Add(cliente);
            }

            //7. Combinar y reordenar
            List<Api_AppMobileApi_GetDataDownloadCustomerResponse> finalList;
            finalList = clientesOrdenados.Count > 0 ? _helperCore.ReordenarLista(allCustomers, clientesOrdenados) : allCustomers.OrderBy(c=>c.Secuencia).ToList();

            //8. Agregar a la lista principal
            Customers.AddRange(finalList);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading customers: {ex.Message}");
            ShowMensajePopUp(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
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

            var oldCustomer = (Api_AppMobileApi_GetDataDownloadCustomerResponse)e.DropItem;
            var oldEntityId = oldCustomer.EntityId;
            var oldPosition = e.ItemHandle;
            var entityId = customer.EntityId;
            var newPosition = e.DropItemHandle;

            // Obtener la lista actual de posiciones
            var parameter = await _repositoryTbParameterSystem.PosMeFindCustomerOrderCustomer();
            var currentPositions = (!string.IsNullOrWhiteSpace(parameter.Value)
                ? JsonConvert.DeserializeObject<List<CustomerOrderShare>>(parameter.Value)
                : []) ?? [];
            if (currentPositions.Count <= 0)
            {
                currentPositions.Add(new CustomerOrderShare
                {
                    EntityId = oldEntityId,
                    Position = oldPosition
                });
                currentPositions.Add(new CustomerOrderShare
                {
                    EntityId = entityId,
                    Position = newPosition
                });
                // Actualizar el parámetro en la base de datos
                parameter.Value = JsonConvert.SerializeObject(currentPositions.OrderBy(v => v.Position).ToList());
            }
            else
            {
                // Eliminar duplicados y preparar la lista
                currentPositions.RemoveAll(v => v.EntityId == entityId);
                currentPositions.RemoveAll(v => v.EntityId == oldEntityId);

                // Agregar la nueva posición
                currentPositions.Add(new CustomerOrderShare
                {
                    EntityId = oldEntityId,
                    Position = oldPosition
                });
                currentPositions.Add(new CustomerOrderShare
                {
                    EntityId = entityId,
                    Position = newPosition
                });

                // Actualizar el parámetro en la base de datos
                parameter.Value = JsonConvert.SerializeObject(currentPositions.OrderBy(v => v.Position).ToList());
            }

            oldCustomer.Secuencia = oldPosition;
            customer.Secuencia = newPosition;
            var task1 = _customerRepositoryTbCustomer.PosMeUpdate(oldCustomer);
            var task2 = _customerRepositoryTbCustomer.PosMeUpdate(customer);
            var update = _repositoryTbParameterSystem.PosMeUpdate(parameter);
            Task.WaitAll(task1, task2, update);
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex.StackTrace);
            ShowMensajePopUp(ex.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}