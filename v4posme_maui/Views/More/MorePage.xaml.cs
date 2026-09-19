using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Microsoft.Maui.Graphics;
using Unity;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.More;
using v4posme_maui.Views.More.Gasto;
using v4posme_maui.Views.More.CashInflow;
using v4posme_maui.Views.More.Logs;
using v4posme_maui.Views.More.Productos;
using v4posme_maui.Views.More.ReporteVenta;
using v4posme_maui.Views.More.Visita;

namespace v4posme_maui.Views.More;

public partial class MorePage : ContentPage
{
	private readonly MoreViewModel moreViewModel;

	public MorePage()
	{
		InitializeComponent();
		moreViewModel = (MoreViewModel)BindingContext;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
	}

	private async void OnCardTapped(object sender, TappedEventArgs e)
	{
		var parameter = e.Parameter?.ToString();
		if (string.IsNullOrEmpty(parameter)) return;

		switch (parameter)
		{
			case "1":
				await Navigation.PushAsync(new ReporteVentaPage());
				break;
			case "2":
				await Navigation.PushAsync(new VisitaPage());
				break;
			case "3":
				await Navigation.PushAsync(new ProductosRetornosPage());
				break;
			case "4":
				await Navigation.PushAsync(new ProductosVendidosPage());
				break;
			case "5":
				await Navigation.PushAsync(new LogsPage());
				break;
			case "6":
				await Navigation.PushAsync(new GastoPage());
				break;
			case "7":
				await Navigation.PushAsync(new CashInflowPage());
				break;
			case "8":
				await AbrirReportesRemotoAsync();
				break;
		}
	}

	// Abre el reporte remoto. Lee el parametro descargado "MOBILE_URL_VIEW_REPORT_REMOTE":
	// si es "false" (o vacio) muestra un mensaje de acceso denegado; en caso contrario
	// interpreta el valor como una URL y la abre en el navegador.
	private async Task AbrirReportesRemotoAsync()
	{
		try
		{
			var repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
			var parametro            = await repositoryParameters.PosMeFindByKey("MOBILE_URL_VIEW_REPORT_REMOTE");
			var valor                = parametro?.Value?.Trim();

			if (string.IsNullOrWhiteSpace(valor) || valor.Equals("false", StringComparison.OrdinalIgnoreCase))
			{
				await MostrarMensajeRojo("No tiene acceso a los reportes remotos");
				return;
			}

			await Browser.OpenAsync(valor, BrowserLaunchMode.SystemPreferred);
		}
		catch (Exception e)
		{
			await MostrarMensajeRojo($"No se pudo abrir el reporte: {e.Message}");
		}
	}

	// Muestra un mensaje estilizado en rojo (snackbar) en la parte inferior, acorde al
	// estilo del resto de la aplicacion.
	private static async Task MostrarMensajeRojo(string mensaje)
	{
		var opciones = new SnackbarOptions
		{
			BackgroundColor      = Color.FromArgb("#E53935"),
			TextColor            = Colors.White,
			CornerRadius         = new CornerRadius(12),
			Font                 = Microsoft.Maui.Font.SystemFontOfSize(14),
			ActionButtonTextColor = Colors.White
		};

		var snackbar = Snackbar.Make(
			mensaje,
			duration: TimeSpan.FromSeconds(3),
			visualOptions: opciones);

		await snackbar.Show();
	}
}