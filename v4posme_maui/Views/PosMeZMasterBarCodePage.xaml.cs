using System.Diagnostics;
using ZXing.Net.Maui;

namespace v4posme_maui.Views;

public partial class BarCodePage : ContentPage
{
    private bool _isAnimating = true;
    private const int StepSize = 100;
    private double _currentY = 0;
    private readonly TaskCompletionSource<string?> _pageResultCompletionSource;

    public BarCodePage()
    {
        InitializeComponent();
        BarcodeReader.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormats.OneDimensional,
            AutoRotate = true,
            Multiple = false
        };
        StartScanAnimation();
        _pageResultCompletionSource = new TaskCompletionSource<string?>();
    }

    public Task<string?> WaitForResultAsync()
    {
        return _pageResultCompletionSource.Task;
    }

    private async void OnBarcodesDetected(object? sender, BarcodeDetectionEventArgs e)
    {
        try
        {
            
            var barCode = e.Results.FirstOrDefault();
            if (barCode is null)
            {
                return;
            }

            if (!_pageResultCompletionSource.Task.IsCompleted)
            {
                _pageResultCompletionSource.SetResult(barCode.Value);
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Navigation.PopModalAsync();
                });
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private async void StartScanAnimation()
    {
        _isAnimating = true;

        while (_isAnimating)
        {
            _currentY += StepSize;
            if (_currentY >= Height)
            {
                _currentY = 0;
            }

            await ScanLine.TranslateTo(0, _currentY, 250, Easing.CubicOut);
        }
    }

    private void StopScanAnimation()
    {
        _isAnimating = false;
        ScanLine.AbortAnimation("TranslateTo");
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopScanAnimation();
    }
}