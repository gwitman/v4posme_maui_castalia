using CommunityToolkit.Maui.Core;
using DevExpress.Maui.Core;
using System.Diagnostics;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.More.ReporteVenta;

namespace v4posme_maui.Views.More.ReporteVenta;

public partial class ReporteVentaPage : ContentPage
{
	private ReporteVentaViewModel viewModel;

	public ReporteVentaPage()
	{
		InitializeComponent();
		viewModel = (ReporteVentaViewModel)BindingContext;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		await viewModel.OnAppearing(Navigation);
		reporteFecha.Text = $"DEL {viewModel.FechaInical:dd/MM/yyyy} - {viewModel.FechaFinal:dd/MM/yyyy}";
	}

	protected override bool OnBackButtonPressed()
	{
		var stack = Shell.Current.Navigation.NavigationStack.ToArray();

		if(viewModel.IsFormVisible && !viewModel.IsVisibleDate)
		{
			viewModel.BackForm();
		}
		else
		{
			for (var i = stack.Length - 1; i > 0; i--)
			{
				Shell.Current.Navigation.RemovePage(stack[i]);
			}
		}

		return true;
	}

	private async void BackToHome_OnClicked(object? sender, EventArgs e)
	{
		Application.Current!.MainPage = new MainPage();
		await Navigation.PopToRootAsync();
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

	private async void Preview_OnClicked(object? sender, EventArgs e)
	{
		try
		{
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
		var screenshotResult = await DxStackLayout_.CaptureAsync();

		if (screenshotResult is null)
		{
			viewModel.ShowToast(Mensajes.MensajeCompartirError, ToastDuration.Long, 18);
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
}