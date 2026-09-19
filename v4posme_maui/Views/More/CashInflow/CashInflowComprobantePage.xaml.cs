using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.More.CashInflow;

namespace v4posme_maui.Views.More.CashInflow;

public partial class CashInflowComprobantePage : ContentPage
{
    private readonly CashInflowComprobanteViewModel _viewModel;

    public CashInflowComprobantePage()
    {
        InitializeComponent();
        _viewModel = (CashInflowComprobanteViewModel)BindingContext;
        _viewModel.CompartirSolicitado += OnCompartirSolicitado;
        _viewModel.EliminacionCompletada += OnEliminacionCompletada;
    }

    private async void OnEliminacionCompletada(object? sender, bool eliminado)
    {
        if (!eliminado) return;

        // Tras eliminar se regresa al inicio (la transaccion ya no existe).
        Application.Current!.MainPage = new MainPage();
        await Navigation.PopToRootAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
    }

    // Bloquea el boton atras: solo se puede salir por Inicio o Nuevo Ingreso.
    protected override bool OnBackButtonPressed()
    {
        var stack = Shell.Current.Navigation.NavigationStack.ToArray();
        for (var i = stack.Length - 1; i > 0; i--)
        {
            Shell.Current.Navigation.RemovePage(stack[i]);
        }
        return true;
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
            Title = "Compartir Ingreso",
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
