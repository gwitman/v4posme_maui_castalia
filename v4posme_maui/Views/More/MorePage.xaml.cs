using v4posme_maui.ViewModels.More;
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

	private async void OnButtonClicked(object sender, EventArgs e)
	{
		if (sender is not Button button) return;
		var parameter = button.CommandParameter;
		if (parameter is null)
		{
			return;
		}
		switch (parameter.ToString())
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
		}
	}
}