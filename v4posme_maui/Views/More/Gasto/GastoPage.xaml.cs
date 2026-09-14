using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.More.Gasto;

namespace v4posme_maui.Views.More.Gasto;

public partial class GastoPage : ContentPage
{
    private readonly GastoViewModel _viewModel;

    public GastoPage()
    {
        InitializeComponent();
        _viewModel = (GastoViewModel)BindingContext;
        _viewModel.CompartirSolicitado += OnCompartirSolicitado;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.OnAppearing(Navigation);
    }

    private async void BackToHome_OnClicked(object? sender, EventArgs e)
    {
        Application.Current!.MainPage = new MainPage();
        await Navigation.PopToRootAsync();
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        _viewModel.PopUpShow = false;
    }

    private async void OnCompartirSolicitado(object? sender, EventArgs e)
    {
        try
        {
            var filePath = await FileImage();
            if (string.IsNullOrWhiteSpace(filePath)) return;
            await ShareImageAsync(filePath);
        }
        catch (Exception exception)
        {
            HelperLogs.Log(exception);
            Debug.WriteLine(exception);
        }
    }

    private async Task ShareImageAsync(string imagePath)
    {
        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir Gasto",
            File = new ShareFile(imagePath)
        });
    }

    private async Task<string> FileImage()
    {
        var screenshotResult = await DxStackLayout_.CaptureAsync();
        if (screenshotResult is null)
        {
            _viewModel.ShowToast(Mensajes.MensajeCompartirError, ToastDuration.Long, 18);
            return "";
        }

        await using var stream = await screenshotResult.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);

        var dateTime = DateTime.Now;
        var result   = $"{dateTime.Year}{dateTime.Month}{dateTime.Day}{dateTime.Hour}{dateTime.Minute}{dateTime.Second}";
        var filePath = GetFilePath($"{result}.png");
        await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());

        return filePath;
    }

    private static string GetFilePath(string filename)
    {
        var folderPath = Environment.GetFolderPath(DeviceInfo.Platform == DevicePlatform.Android
            ? Environment.SpecialFolder.LocalApplicationData
            : Environment.SpecialFolder.MyDocuments);

        return Path.Combine(folderPath, filename);
    }
}
