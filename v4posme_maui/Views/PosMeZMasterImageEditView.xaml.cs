using System.Diagnostics;
using System.Globalization;
using DevExpress.Maui.Editors;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using ImageFormat = DevExpress.Maui.Editors.ImageFormat;

namespace v4posme_maui.Views;

public partial class ImageEditView : ContentPage
{
    private readonly TaskCompletionSource<ImageSource?> _pageResultCompletionSource;
    public string Imagen = string.Empty;

    public ImageEditView()
    {
        InitializeComponent();
        _pageResultCompletionSource = new TaskCompletionSource<ImageSource?>();
    }

    public ImageEditView(ImageSource imageSource)
    {
        InitializeComponent();
        _pageResultCompletionSource = new TaskCompletionSource<ImageSource?>();
        Editor.Source = imageSource;
    }

    public Task<ImageSource?> WaitForResultAsync()
    {
        return _pageResultCompletionSource.Task;
    }

    private async void BackPressed(object sender, EventArgs e)
    {
        _pageResultCompletionSource.SetResult(null);
        await Navigation.PopAsync();
    }

    private async void CropPressed(object sender, EventArgs e)
    {
        try
        {
            VariablesGlobales.LogoTemp = Editor.SaveAsBase64(ImageFormat.Jpeg);
            _pageResultCompletionSource.SetResult(Editor.SaveAsImageSource());
            await Navigation.PopAsync();
        }
        catch (Exception exception)
        {
            Debug.WriteLine(exception);
        }
    }
}

public class FrameTypeToImageStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (CropAreaShape)value == CropAreaShape.Ellipse ? "ic_frame_rect" : "ic_frame_circle";
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}