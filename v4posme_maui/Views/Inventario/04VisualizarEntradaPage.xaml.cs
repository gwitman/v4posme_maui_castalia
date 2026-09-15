using System.Diagnostics;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.Inventario;
using Unity;

namespace v4posme_maui.Views.Inventario;

public partial class VisualizarEntradaPage : ContentPage
{
    private readonly HelperCore _helperCore;

    public VisualizarEntradaPage()
    {
        InitializeComponent();
        _helperCore = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        if (BindingContext is VisualizarEntradaViewModel vm)
            vm.CompartirSolicitado += OnCompartirSolicitado;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is VisualizarEntradaViewModel vm)
            vm.OnAppearing(Navigation);
    }

    private async void OnCompartirSolicitado(object? sender, EventArgs e)
    {
        try
        {
            var screenshotResult = await ShareLayout.CaptureAsync();
            var dateTime         = DateTime.Now;
            var name             = $"{dateTime.Year}{dateTime.Month}{dateTime.Day}{dateTime.Hour}{dateTime.Minute}{dateTime.Second}";
            var filePath         = await _helperCore.FileImage(screenshotResult, $"{name}.png");
            if (string.IsNullOrWhiteSpace(filePath)) return;

            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Compartir Compra",
                File  = new ShareFile(filePath)
            });
        }
        catch (Exception exception)
        {
            HelperLogs.Log(exception);
            Debug.WriteLine(exception);
        }
    }
}
