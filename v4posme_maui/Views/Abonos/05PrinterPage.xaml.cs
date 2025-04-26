using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.Abonos;

namespace v4posme_maui.Views.Abonos;

public partial class ValidarAbonoPage : ContentPage
{
    //IBluetoothLE ble;
    //IAdapter adapter;
    //IDevice printerDevice;
    public ValidarAbonoPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ((ValidarAbonoViewModel)BindingContext).OnAppearing(Navigation);
        Logo.Source = ((ValidarAbonoViewModel)BindingContext).LogoSource;
    }

    protected override bool OnBackButtonPressed()
    {
        var stack = Shell.Current.Navigation.NavigationStack.ToArray();
        for (var i = stack.Length - 1; i > 0; i--)
        {
            Shell.Current.Navigation.RemovePage(stack[i]);
        }
        return true;
    }
    
    private async void MenuItem_OnClicked(object? sender, EventArgs e)
    {
        try
        {
            var filePath = await FileImage();
            await ShareImageAsync(filePath);
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }

    private string GetFilePath(string filename)
    {
        string folderPath;

        folderPath = Environment.GetFolderPath(DeviceInfo.Platform == DevicePlatform.Android
            ? Environment.SpecialFolder.LocalApplicationData
            : Environment.SpecialFolder.MyDocuments);

        return Path.Combine(folderPath, filename);
    }

    private async Task ShareImageAsync(string imagePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir Comprobante de abono",
            File = new ShareFile(imagePath)
        });
    }

    private async Task<string> FileImage()
    {
        var screenshotResult = await DxStackLayout.CaptureAsync();
        if (screenshotResult is null)
        {
            ((ValidarAbonoViewModel)BindingContext)
                .ShowToast(Mensajes.MensajeCompartirError,
                    ToastDuration.Long, 18);
            return "";
        }

        await using var stream = await screenshotResult.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var dateTime = DateTime.Now;
        var result = $"{dateTime.Year}{dateTime.Month}{dateTime.Day}{dateTime.Hour}{dateTime.Minute}{dateTime.Second}";
        var filePath = GetFilePath($"{result}.png");
        await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
        return filePath;
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}