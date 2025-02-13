using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core;
using SkiaSharp;
using Unity;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.Printers;

namespace v4posme_maui.Views.Printers;

public partial class PrinterProductoPage : ContentPage
{
    private readonly PrinterProductViewModel _printerProductViewModel;
    private HelperCore _helperCore;
    
    public PrinterProductoPage()
    {
        InitializeComponent();
        _printerProductViewModel = (PrinterProductViewModel)BindingContext;
        _helperCore = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _printerProductViewModel.OnAppearing(Navigation);
    }

    private async void MenuItem_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            var dateTime = DateTime.Now;
            var screenshotResult = await DxStackLayout.CaptureAsync();
            var result = $"{dateTime.Year}{dateTime.Month}{dateTime.Day}{dateTime.Hour}{dateTime.Minute}{dateTime.Second}";
            var filePath = await _helperCore.FileImage(screenshotResult, $"{result}.png");
            await ShareImageAsync(filePath);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private async Task ShareImageAsync(string imagePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir Comprobante de abono",
            File = new ShareFile(imagePath)
        });
    }

    private async void Printer_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            var barcodeResult = await BarcodeGeneratorView.CaptureAsync();
            var imageBarcode = await _helperCore.FileImage(barcodeResult, "barcode.png");
            var bitmapBarcode = SKBitmap.Decode(imageBarcode);
            _printerProductViewModel.OnPrinterCommand(bitmapBarcode);
        }
        catch (Exception ex)
        {
            _printerProductViewModel.ShowToast(ex.Message, ToastDuration.Long, 18);
        }
    }
}