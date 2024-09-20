using System.Text;
using DevExpress.Maui.Controls;
using v4posme_maui.Services.Helpers;
using v4posme_maui.ViewModels;
using v4posme_maui.Services.SystemNames;
using Debug = System.Diagnostics.Debug;

namespace v4posme_maui.Views;

public partial class ParameterPage : ContentPage
{
    private readonly PosMeParameterViewModel _viewModel;

    public ParameterPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new PosMeParameterViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }

    private void ImageTapped(object sender, EventArgs e)
    {
        BottomSheet.State = BottomSheetState.HalfExpanded;
    }

    private async void DeletePhotoClicked(object sender, EventArgs args)
    {
        await Dispatcher.DispatchAsync(() =>
        {
            BottomSheet.State = BottomSheetState.Hidden;
            EditControl.IsVisible = false;
            Preview.Source = null;
        });
    }

    private async void SelectPhotoClicked(object sender, EventArgs args)
    {
        var photo = await MediaPicker.PickPhotoAsync();
        await ProcessResult(photo);
        EditControl.IsVisible = true;
    }

    private async void TakePhotoClicked(object sender, EventArgs args)
    {
        if (!MediaPicker.Default.IsCaptureSupported)
            return;

        var photo = await MediaPicker.Default.CapturePhotoAsync();
        await ProcessResult(photo);
        EditControl.IsVisible = true;
    }

    private async Task ProcessResult(FileResult? result)
    {
        await Dispatcher.DispatchAsync(() => { BottomSheet.State = BottomSheetState.Hidden; });

        if (result == null)
            return;

        var imageSource = ImageSource.FromFile(result.FullPath);
        var encoding = Encoding.UTF8;
        var stream = await result.OpenReadAsync();
        var streamReader = new StreamReader(stream, encoding);
        var memoryStream = new MemoryStream();
        await streamReader.BaseStream.CopyToAsync(memoryStream);
        var imageBytes = memoryStream.ToArray();
        VariablesGlobales.LogoTemp = Convert.ToBase64String(imageBytes);
        Preview.Source = imageSource;
        streamReader.Close();
        memoryStream.Close();
    }

    private void RefreshView_OnRefreshing(object? sender, EventArgs e)
    {
        OnAppearing();
        Preview.Source = _viewModel.ShowImage;
    }
}