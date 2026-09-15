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

    private async void Compartir_OnClicked(object? sender, EventArgs e)
    {
        await CompartirImagenAsync();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is VisualizarEntradaViewModel vm)
            vm.OnAppearing(Navigation);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Si esta pantalla se abrio desde Impresiones (solo lectura) y el usuario sale de
        // ella (por ejemplo usando el menu lateral), se elimina de la pila de navegacion
        // para que al regresar a Impresiones se muestren los tabs y no esta visualizacion.
        if (VariablesGlobales.DtoInventario.AbiertoDesdeImpresiones
            && Navigation.NavigationStack.Contains(this))
        {
            Navigation.RemovePage(this);
        }
    }

    private async void OnCompartirSolicitado(object? sender, EventArgs e)
    {
        await CompartirImagenAsync();
    }

    private async Task CompartirImagenAsync()
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
