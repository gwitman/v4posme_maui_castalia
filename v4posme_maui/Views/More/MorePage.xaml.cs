using v4posme_maui.ViewModels.More;
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
		}
	}
}