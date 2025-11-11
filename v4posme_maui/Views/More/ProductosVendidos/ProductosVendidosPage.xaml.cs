using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.More.ProductosVendidos;

namespace v4posme_maui.Views.More.Productos;

public partial class ProductosVendidosPage : ContentPage
{
    private ProductosVendidosViewModel _viewModel;

    public ProductosVendidosPage()
    {
        InitializeComponent();
        _viewModel = (ProductosVendidosViewModel)BindingContext;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.LoadData();
    }

    private async Task ShareImageAsync(string imagePath)
    {
        if (!File.Exists(imagePath))
        {
            _viewModel.Mensaje = Mensajes.ArchivoNoExiste;
            _viewModel.PopupBackgroundColor = Colors.Red;
            _viewModel.PopUpShow = true;
            return;
        }

        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Compartir Prodcutos vendidos",
                File = new ShareFile(imagePath)
            });
        }
        catch (Exception ex)
        {
            _viewModel.Mensaje = $"Error al compartir: {ex.Message}";
            _viewModel.PopupBackgroundColor = Colors.Red;
            _viewModel.PopUpShow = true;


        }
        finally
        {
            File.Delete(imagePath);
        }
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

    private async Task<string> FileImage()
    {
        var screenshotResult = await ItemsGridView.CaptureAsync();
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

    private string GetFilePath(string filename)
    {
        var folderPath = Environment.GetFolderPath(DeviceInfo.Platform == DevicePlatform.Android
            ? Environment.SpecialFolder.LocalApplicationData
            : Environment.SpecialFolder.MyDocuments);

        return Path.Combine(folderPath, filename);
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }
}