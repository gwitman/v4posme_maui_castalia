﻿using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using DevExpress.Maui.CollectionView;
using DevExpress.Maui.Core.Internal;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Invoices;

public class InvoicesViewModel : BaseViewModel
{
    private readonly IRepositoryTbCustomer _customerRepositoryTbCustomer;
    private readonly IRepositoryTbParameterSystem _repositoryTbParameterSystem;
	private readonly HelperCore _helper;
	public ICommand ItemTapped { get; }
    public ICommand SearchCommand { get; }
    public ICommand LoadMoreCommand { get; }
    public ICommand OnBarCode { get; }
    public Api_AppMobileApi_GetDataDownloadCustomerResponse? SelectedCustomer { get; set; }
    private List<CustomerOrderShare> _customerOrderShares = new();
    private int _loadBatchSize = 10;
    private int _lastLoadedIndex;
    
    public InvoicesViewModel()
    {
        Title                           = "Selección de cliente 1/6";
        _customerRepositoryTbCustomer   = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        _repositoryTbParameterSystem    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
		_helper         = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
		ItemTapped      = new Command<Api_AppMobileApi_GetDataDownloadCustomerResponse>(OnItemTapped);
        SearchCommand   = new Command(OnSearchCommand);
        OnBarCode       = new Command(OnBarCodeShow);
        LoadMoreCommand = new Command(OnLoadMoreCommand);
        _customers      = new();
    }

    private DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> _customers;

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse> Customers
    {
        get=>_customers;
        set=>SetProperty(ref _customers, value);
    }


    private void OnLoadMoreCommand()
    {
        if (_lastLoadedIndex == 0)
        {
        }
        else
        {
            LoadCustomers();
        }
    }

    private async Task<List<CustomerOrderShare>> LoadOrderCustomer()
    {
        var customerOrderJson                   = await _repositoryTbParameterSystem.PosMeFindCustomerOrderInvoice();
        List<CustomerOrderShare> customOrder    = [];

        if (!string.IsNullOrWhiteSpace(customerOrderJson.Value))
        {
            customOrder = JsonConvert.DeserializeObject<List<CustomerOrderShare>>(customerOrderJson.Value) ?? [];
        }
        return customOrder;
    }
    
    private async void OnBarCodeShow()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar     = await barCodePage.WaitForResultAsync();
        Search      = bar!;
        if (string.IsNullOrWhiteSpace(Search)) return;
        OnSearchCommand(Search);
    }

    private async void OnItemTapped(Api_AppMobileApi_GetDataDownloadCustomerResponse? item)
    {
        if (item is null)
        {
            return;
        }

        IsBusy = true;
        VariablesGlobales.DtoInvoice = new ViewTempDtoInvoice
        {
            FirstName = item.FirstName,
            LastName = item.LastName,
            Balance = item.Balance
        };
        await NavigationService.NavigateToAsync<DataInvoicesViewModel>(item.CustomerNumber!);
        IsBusy = false;
    }

    private void OnSearchCommand(object? obj)
    {
        IsBusy = true;
        if (obj is not null)
        {
            Search = obj.ToString() ?? string.Empty;
        }
        _lastLoadedIndex = 0;
        LoadCustomers();
        IsBusy = false;
    }
    
    private async Task<bool> LoadCustomers()
    {
        try
        {
            IsBusy = true;
            // 1. Obtener el orden personalizado desde el repositorio
            var customOrder = _customerOrderShares;
            
            // 2. Obtener todos los clientes
            List<Api_AppMobileApi_GetDataDownloadCustomerResponse> allCustomers;
            List<Api_AppMobileApi_GetDataDownloadCustomerResponse> finalList;

            if (_lastLoadedIndex == 0)
            {
                Customers.Clear();
            }

            if (VariablesGlobales.OrdenarClientes)
            {
                await _helper.ReordenarListaClientesFacturas();
                if (string.IsNullOrWhiteSpace(Search))
                {
                    allCustomers = await _customerRepositoryTbCustomer.PosMeCustomerAscLoad(_lastLoadedIndex, _loadBatchSize);
                }
                else
                {
                    allCustomers = await _customerRepositoryTbCustomer.PosMeFilterBySearch(Search, _lastLoadedIndex, _loadBatchSize);
                }

                finalList   = allCustomers;
                if (_lastLoadedIndex == 0)
                {
                    Customers = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(finalList);
                }
                else
                {
                    Customers.AddRange(finalList);
                }

                _lastLoadedIndex    += _loadBatchSize;
                IsBusy              = false;
                
            }
            else
            {
                if (string.IsNullOrWhiteSpace(Search))
                {
                    allCustomers = await _customerRepositoryTbCustomer.PosMeCustomerAscLoad(_lastLoadedIndex, _loadBatchSize);
                }
                else
                {
                    allCustomers = await _customerRepositoryTbCustomer.PosMeFilterBySearch(Search, _lastLoadedIndex, _loadBatchSize);
                }

                
                finalList   = allCustomers;
                if (_lastLoadedIndex == 0)
                {
                    Customers = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadCustomerResponse>(finalList);
                }
                else
                {
                    Customers.AddRange(finalList);
                }

                _lastLoadedIndex    += _loadBatchSize;
                IsBusy              = false;
            }


            

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
            var parameter           = await _repositoryTbParameterSystem.PosMeFindCustomerOrderInvoice();
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
                    EntityId        = cus.EntityId,
                    Position        = cus.Secuencia is null ? 0 : cus?.Secuencia ?? 0,
                    customerNumber  = cus?.CustomerNumber is null ? "" : cus.CustomerNumber
                });
            }


            //Actualizar las posiciones en las tablas
            parameter.Value = JsonConvert.SerializeObject(currentPositions);
            await _customerRepositoryTbCustomer.PosMeUpdateAll(customerList);
            await _repositoryTbParameterSystem.PosMeUpdate(parameter);

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
    
    public async void OnAppearing(INavigation navigation)
    {
        try
        {
            Navigation              = navigation;
            var valueTop            = await _helper.GetValueParameter("MOBILE_SHOW_TOP_CUSTOMER", "10");
            _loadBatchSize          = int.Parse(valueTop);
            _lastLoadedIndex        = 0;
            await LoadCustomers();

        }
        catch (Exception e)
        {
            Debug.WriteLine(e.StackTrace);
            ShowMensajePopUp(e.Message);
        }
        finally
        {
            IsBusy = false;
        }
    }
}