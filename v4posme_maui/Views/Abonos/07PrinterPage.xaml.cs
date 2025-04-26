using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.Abonos;

namespace v4posme_maui.Views.Abonos;

public partial class ValidarAbonoFinancieraPage : ContentPage
{
    private readonly ValidarAbonoFinancieraViewModel _viewModel;

    public ValidarAbonoFinancieraPage()
    {
        InitializeComponent();
        _viewModel = (ValidarAbonoFinancieraViewModel)BindingContext;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
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
        var folderPath = Environment.GetFolderPath(DeviceInfo.Platform == DevicePlatform.Android
            ? Environment.SpecialFolder.LocalApplicationData
            : Environment.SpecialFolder.MyDocuments);

        return Path.Combine(folderPath, filename);
    }

    private async Task ShareImageAsync(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            _viewModel.ShowToast("El archivo no existe", ToastDuration.Long, 18);
            return;
        }

        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Compartir Comprobante de abono",
                File = new ShareFile(imagePath)
            });
        }
        catch (Exception ex)
        {
            _viewModel.ShowToast($"Error al compartir: {ex.Message}", ToastDuration.Long, 18);
        }
        finally
        {
            File.Delete(imagePath);
        }
    }

    private async Task<string> FileImage()
    {
        var screenshotResult = await DxStackLayout.CaptureAsync();
        if (screenshotResult is null)
        {
            _viewModel.ShowToast(Mensajes.MensajeCompartirError, ToastDuration.Long, 18);
            return "";
        }

        // Guardar directamente en un archivo sin MemoryStream intermedio
        var dateTime = DateTime.Now;
        var result = $"{dateTime.Year}{dateTime.Month}{dateTime.Day}{dateTime.Hour}{dateTime.Minute}{dateTime.Second}";
        var filePath = GetFilePath($"{result}.png");

        await using var stream = await screenshotResult.OpenReadAsync();
        await using var fileStream = File.Create(filePath);
        await stream.CopyToAsync(fileStream);

        return filePath;
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}