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
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.Abonos;

public class AbonosViewModel : BaseViewModel
{
    private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;
    private readonly IRepositoryTbParameterSystem _repositoryTbParameterSystem;

    public AbonosViewModel()
    {
        Title = "Selección de cliente 1/5";
        _customerRepositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        _repositoryTbParameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _customers = new();
        SearchCommand = new Command(OnSearchCommand);
        OnBarCode = new Command(OnBarCodeShow);
        ItemTapped = new Command<Api_AppMobileApi_GetDataDownloadCustomerResponse>(OnItemSelected);
    }

    private async void OnBarCodeShow()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar = await barCodePage.WaitForResultAsync();
        Search = bar!;
        if (string.IsNullOrWhiteSpace(Search)) return;
        OnSearchCommand();
    }

    private async void OnItemSelected(Api_AppMobileApi_GetDataDownloadCustomerResponse? item)
    {
        if (item is null)
        {
            return;
        }

        IsBusy = true;
        var invoices = await _repositoryDocumentCredit.PosMeFindByEntityId(item.EntityId);
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
        IsBusy = true;
        List<Api_AppMobileApi_GetDataDownloadCustomerResponse> finds;
        if (string.IsNullOrWhiteSpace(Search))
        {
            finds = await _customerRepositoryTbCustomer.PosMeFilterByInvoice();
        }
        else
        {
            finds = await _customerRepositoryTbCustomer.PosMeFilterByCustomerInvoice(Search);
        }

        Customers.Clear();
        foreach (var customer in finds)
        {
            Customers.Add(customer);
        }

        IsBusy = false;
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
            IsBusy = true;
            // 1. Obtener el orden personalizado desde el repositorio
            var customerOrderJson = await _repositoryTbParameterSystem.PosMeFindCustomerOrderShare();
            List<CustomerOrderShare> customOrder = [];

            if (!string.IsNullOrWhiteSpace(customerOrderJson.Value))
            {
                customOrder = JsonConvert.DeserializeObject<List<CustomerOrderShare>>(customerOrderJson.Value) ?? [];
            }

            // 2. Obtener todos los clientes
            var allCustomers = await _customerRepositoryTbCustomer.PosMeFilterByInvoice();
            var remainingCustomers = new List<Api_AppMobileApi_GetDataDownloadCustomerResponse>();
            for (var i = 0; i < allCustomers.Count; i++)
            {
                var c = allCustomers[i];
                c.Secuencia = i;
                remainingCustomers.Add(c);
            }

            // 3. Si no hay orden personalizado, cargar directamente
            if (customOrder.Count == 0)
            {
                Customers  = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(allCustomers);
                IsBusy     = false;
                return;
            }

            //5. Inicializar lista de clientes ordenados
            var clientesOrdenados = new List<Api_AppMobileApi_GetDataDownloadCustomerResponse>();

            //6. Colocar los personalizados
            foreach (var personalizado in customOrder.OrderBy(c => c.Position))
            {
                var cliente = remainingCustomers.FirstOrDefault(c => c.EntityId == personalizado.EntityId);
                if (cliente == null) continue;
                cliente.Secuencia = personalizado.Position;
                clientesOrdenados.Add(cliente);
            }

            //7. Combinar y reordenar
            var finalList = ReordenarLista(remainingCustomers, clientesOrdenados);

            //8. Agregar a la lista principal
            Customers = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(finalList);
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

    private static List<Api_AppMobileApi_GetDataDownloadCustomerResponse> ReordenarLista(List<Api_AppMobileApi_GetDataDownloadCustomerResponse> listaBase,
        List<Api_AppMobileApi_GetDataDownloadCustomerResponse> cambios)
    {
        // Paso 1: Aplicar los cambios de secuencia
        foreach (var cambio in cambios)
        {
            var cliente = listaBase.FirstOrDefault(c => c.EntityId == cambio.EntityId);
            if (cliente != null)
            {
                cliente.Secuencia = cambio.Secuencia;
            }
        }

        // Paso 2: Ordenar por secuencia y luego por EntityId para desempate
        var listaOrdenada = listaBase
            .OrderBy(c => c.Secuencia)
            .ThenBy(c => c.EntityId)
            .ToList();

        // Paso 3: Reasignar secuencias para que sean consecutivas
        var nuevaSecuencia = 1;
        foreach (var cliente in listaOrdenada)
        {
            cliente.Secuencia = nuevaSecuencia++;
        }

        return listaOrdenada;
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

            var oldEntityId = ((Api_AppMobileApi_GetDataDownloadCustomerResponse)e.DropItem).EntityId;
            var oldPosition = e.ItemHandle;
            var entityId = customer.EntityId;
            var newPosition = e.DropItemHandle;

            // Obtener la lista actual de posiciones
            var parameter = await _repositoryTbParameterSystem.PosMeFindCustomerOrderShare();
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

            await _repositoryTbParameterSystem.PosMeUpdate(parameter);
            LoadsClientes();
        }
        catch (Exception ex)
        {
            ShowToast("Error al guardar la posición: " + ex.Message, ToastDuration.Long, 14);
        }
    }
}