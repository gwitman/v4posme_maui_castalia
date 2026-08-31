﻿using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core;
using DevExpress.Maui.Core.Internal;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Invoices;

public class SeleccionarProductoViewModel : BaseViewModel
{
    private readonly IRepositoryItems _repositoryItems;
    private readonly HelperCore _helper;

    public SeleccionarProductoViewModel()
    {
        Title                         = "Seleccionar producto 4/6";
        Productos                     = new();
        _repositoryItems              = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        AnadirProducto                = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnAnadirProducto);
        _helper                       = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        SearchBarCodeCommand          = new Command(OnSearchBarCode);
        SearchCommand                 = new Command(OnSearch);
        ProductosSeleccionadosCommand = new Command(OnRevisarProductos);
        QuitarProductoCommand         = new Command<Api_AppMobileApi_GetDataDownloadItemsResponse>(OnQuitarProducto);
    }

    private async void OnRevisarProductos(object obj)
    {
        if (VariablesGlobales.DtoInvoice.Items.Count <= 0)
        {
            ShowToast(Mensajes.MensajeSeleccionarProductos, ToastDuration.Long, 12);
            return;
        }
        try
        {
            IsBusy = true;
            await NavigationService.NavigateToAsync<RevisarProductosSeleccionadosViewModel>();
        }
        catch (Exception ex)
        {
            ShowToast($"Error al navegar: {ex.Message}", ToastDuration.Long, 12);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnSearch()
    {
        if (string.IsNullOrWhiteSpace(Search))
        {
            IsPanelVisible = !IsPanelVisible;
            return;
        }

        IsPanelVisible = !IsPanelVisible;
        await LoadAllProductosAsync();
    }

    private async Task LoadAllProductosAsync()
    {
        IsBusy = true;
        try
        {
            List<Api_AppMobileApi_GetDataDownloadItemsResponse> items;

            if (string.IsNullOrWhiteSpace(Search))
            {
                items = await _repositoryItems.PosMeFindAll();
            }
            else
            {
                items = await _repositoryItems.PosMeFilterdByItemNumberAndBarCodeAndName(Search);
            }

            items = items.OrderBy(i => i.Name).ToList();

            foreach (var item in items)
            {
                item.Name          = item.Name?.ToLower();
                item.MonedaSimbolo = VariablesGlobales.DtoInvoice.Currency!.Simbolo;
            }

            Productos.Clear();
            Productos.AddRange(items);
        }
        catch (Exception ex)
        {
            ShowToast($"Error al cargar productos: {ex.Message}", ToastDuration.Long, 12);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async void OnSearchBarCode()
    {
        var barCodePage = new BarCodePage();
        await Navigation!.PushModalAsync(barCodePage);
        var bar         = await barCodePage.WaitForResultAsync();
        Search          = bar!;
        IsPanelVisible  = !IsPanelVisible;
    }

    private async void OnAnadirProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        var permitirRepetidos         = await _helper.GetValueParameter("MOBILE_ALLOW_REPEATED_PRODUCTS", "false");
        var cestaArticulos            = VariablesGlobales.DtoInvoice.Items;
        var transactionMasterDetailID = _helper.GetTimestampId();

        if (permitirRepetidos == "true")
        {
            var nuevo = new Api_AppMobileApi_GetDataDownloadItemsResponse
            {
                TransactionMasterDetailID = transactionMasterDetailID,
                ItemPk              = obj.ItemPk,
                ItemId              = obj.ItemId,
                BarCode             = obj.BarCode,
                ItemNumber          = obj.ItemNumber,
                Name                = obj.Name,
                PrecioPublico       = obj.PrecioPublico,
                CantidadEntradas    = obj.CantidadEntradas,
                CantidadSalidas     = obj.CantidadSalidas,
                CantidadFinal       = obj.CantidadFinal,
                MonedaSimbolo       = obj.MonedaSimbolo,
                Quantity            = decimal.One,
                MontoDescuento      = 0m,
                PorcentajeDescuento = 0m
            };
            cestaArticulos.Add(nuevo);
        }
        else
        {
            var find = cestaArticulos.FirstOrDefault(response => response.ItemNumber == obj.ItemNumber);
            if (find is not null)
            {
                find.Quantity       += decimal.One;
                find.Importe        = find.PrecioPublico * find.Quantity;
                find.MontoDescuento = find.PorcentajeDescuento > 0
                    ? find.Importe * (find.PorcentajeDescuento / 100m)
                    : find.MontoDescuento;
            }
            else
            {
                obj.TransactionMasterDetailID = transactionMasterDetailID;
                obj.Quantity                  = decimal.One;
                obj.Importe                   = obj.PrecioPublico;
                obj.MontoDescuento            = 0m;
                cestaArticulos.Add(obj);
            }
        }

        VariablesGlobales.DtoInvoice.Balance = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);
        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada++;
        ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
        ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";
    }

    private void OnQuitarProducto(Api_AppMobileApi_GetDataDownloadItemsResponse? obj)
    {
        if (obj is null) return;

        var cestaArticulos = VariablesGlobales.DtoInvoice.Items;
        var find           = cestaArticulos.FirstOrDefault(response => response.ItemNumber == obj.ItemNumber);
        if (find is null) return;

        if (find.Quantity > decimal.One)
        {
            find.Quantity       -= decimal.One;
            find.Importe        = find.PrecioPublico * find.Quantity;
            find.MontoDescuento = find.PorcentajeDescuento > 0
                ? find.Importe * (find.PorcentajeDescuento / 100m)
                : find.MontoDescuento;
        }
        else
        {
            cestaArticulos.Remove(find);
        }

        VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada = cestaArticulos.Count;
        VariablesGlobales.DtoInvoice.Balance = cestaArticulos.Sum(r => r.Importe) - cestaArticulos.Sum(r => r.MontoDescuento);
        ProductosSeleccionadosCantidad = cestaArticulos.Count > 0
            ? $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items"
            : "Seleccionar Productos";
        ProductosSeleccionadosCantidadTotal = cestaArticulos.Count > 0
            ? $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}"
            : "Items";
    }

    public async void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        await LoadAllProductosAsync();

        if (VariablesGlobales.DtoInvoice.Items.Count > 0)
        {
            ProductosSeleccionadosCantidad      = $"Enviar {VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items";
            ProductosSeleccionadosCantidadTotal = $"{VariablesGlobales.DtoInvoice.CantidadTotalSeleccionada} Items = {VariablesGlobales.DtoInvoice.Balance}";
        }
    }

    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Productos { get; }

    private string _productosSeleccionadosCantidadTotal = "Items";
    public string ProductosSeleccionadosCantidadTotal
    {
        get => _productosSeleccionadosCantidadTotal;
        set => SetProperty(ref _productosSeleccionadosCantidadTotal, value);
    }

    private string _productosSeleccionadosCantidad = "Seleccionar Productos";
    public string ProductosSeleccionadosCantidad
    {
        get => _productosSeleccionadosCantidad;
        set => SetProperty(ref _productosSeleccionadosCantidad, value);
    }

    private int _cantidad;
    public int Cantidad
    {
        get => _cantidad;
        set => SetProperty(ref _cantidad, value);
    }

    public Command AnadirProducto { get; }
    public Command SearchCommand { get; }
    public Command SearchBarCodeCommand { get; }
    public Command<Api_AppMobileApi_GetDataDownloadItemsResponse> QuitarProductoCommand { get; }

    private bool _isPanelVisible;
    public bool IsPanelVisible
    {
        get => _isPanelVisible;
        set => SetProperty(ref _isPanelVisible, value);
    }

    public Command ProductosSeleccionadosCommand { get; }
}
