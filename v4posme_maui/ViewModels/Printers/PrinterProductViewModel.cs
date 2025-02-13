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
using static Android.Util.Xml;
using static Android.Renderscripts.ScriptGroup;
using v4posme_maui.Services.Helpers;

namespace v4posme_maui.ViewModels.Printers;

public class PrinterProductViewModel : BaseViewModel
{
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryParameters _repositoryParameters;
    private readonly HelperCore _helperCore;

    public PrinterProductViewModel()
    {
        Title = "Detalle de Producto";
        _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _helperCore = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        ClearCommand = new Command(OnClearCommand);
    }

    private void OnClearCommand(object obj)
    {
        CantidadImprimir = 1;
    }

    public async void OnPrinterCommand(SKBitmap bitmapBarcode)
    {
        try
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

            string barCode  = _helperCore.FormatString(item.BarCode.Replace("T", "").Replace("I","").Replace("B",""));
            barCode         = _helperCore.ConcatenatePairs(barCode);            
            printer.Code128(barCode, Services.HelpersPrinters.Enums.Positions.BelowBarcode);            
            printer.Avanza(85 /*8puntos = 1mm*/);
            for (int i = 1; i <= CantidadImprimir * 2 ; i++)
            {
                printer.Print(); 
            }
        
            if (printer.Device is null)
            {
                ShowToast(Mensajes.MensajeDispositivoNoConectado, ToastDuration.Long, 18);
            }
        }
        catch (Exception e)
        {
            ShowToast(e.Message, ToastDuration.Long, 18);
        }
    }

   

    private Api_AppMobileApi_GetDataDownloadItemsResponse _itemsResponse;

    public Api_AppMobileApi_GetDataDownloadItemsResponse ItemsResponse
    {
        get => _itemsResponse;
        set => SetProperty(ref _itemsResponse, value);
    }

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