using AndroidX.Lifecycle;
using v4posme_maui.ViewModels.More;
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

	private async void OnButtonClicked(object sender, EventArgs e)
	{
		if (sender is Button button)
		{
			var parameter = button.CommandParameter; // Aquí obtienes el parámetro

			if (parameter.ToString() == "2")
			{
				await Navigation.PushAsync(new VisitaPage());
			}
			else if (parameter.ToString() == "1")
			{
				await Navigation.PushAsync(new ReporteVentaPage());
			}
		}
	}
}