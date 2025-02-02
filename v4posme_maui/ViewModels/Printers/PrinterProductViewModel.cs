using CommunityToolkit.Maui.Core;
using Plugin.BLE;
using v4posme_maui.Models;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using SkiaSharp;
using Unity;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace v4posme_maui.ViewModels.Printers;

public class PrinterProductViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryParameters _repositoryParameters;

    public PrinterProductViewModel()
    {
        Title = "Detalle de Producto";
        _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        PrinterCommand = new Command(OnPrinterCommand);
        ClearCommand = new Command(OnClearCommand);
    }

    private void OnClearCommand(object obj)
    {
        CantidadImprimir = 1;
    }

    private async void OnPrinterCommand(object obj)
    {
        if (CantidadImprimir ==0)
        {
            ShowToast(Mensajes.MensajeCantidadImprimir, ToastDuration.Long, 14);
            return;
        }
        var parametroPrinter = await _parameterSystem.PosMeFindPrinter();
        if (string.IsNullOrWhiteSpace(parametroPrinter.Value))
        {
            return;
        }

        var item = VariablesGlobales.Item;
        var printer = new Printer(parametroPrinter.Value);
        if (!CrossBluetoothLE.Current.IsOn)
        {
            ShowToast(Mensajes.MensajeBluetoothState, ToastDuration.Long, 18);
            return;
        }

        //printer.Code39CustomPosMe2px1p(item.BarCode);
        printer.Code128(item.BarCode);
        /*printer.Append(item.Name);*/
        printer.Append(item.BarCode);
        /*printer.Append(item.PrecioPublico.ToString("N2"));*/
        printer.Append("-");
        printer.Avanza(45 /*8puntos = 1mm*/);
        for (int i = 1; i <= CantidadImprimir * 2 ; i++)
        {
            printer.Print(); 
        }
        
        if (printer.Device is null)
        {
            ShowToast(Mensajes.MensajeDispositivoNoConectado, ToastDuration.Long, 18);
        }
    }

    private Api_AppMobileApi_GetDataDownloadItemsResponse _itemsResponse;

    public Api_AppMobileApi_GetDataDownloadItemsResponse ItemsResponse
    {
        get => _itemsResponse;
        set => SetProperty(ref _itemsResponse, value);
    }

    public Command PrinterCommand { get; }
    private int _cantidadImprimir = 1;

    public int CantidadImprimir
    {
        get => _cantidadImprimir;
        set => SetProperty(ref _cantidadImprimir, value);
    }

    public Command ClearCommand { get; }

    public void OnAppearing(INavigation navigation)
    {
        Navigation = navigation;
        ItemsResponse = VariablesGlobales.Item;
        IsBusy = false;
    }
}