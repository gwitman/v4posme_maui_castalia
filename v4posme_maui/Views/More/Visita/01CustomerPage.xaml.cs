using CommunityToolkit.Maui.Views;
using v4posme_maui.Models;
using v4posme_maui.ViewModels.Abonos;
using v4posme_maui.ViewModels.More.Visita;
using v4posme_maui.Views.Abonos;
using v4posme_maui.Views.Invoices;

namespace v4posme_maui.Views.More.Visita;

public partial class VisitaPage : ContentPage
{

    private readonly VisitaViewModel _viewModel;
    public VisitaPage()
    {
        InitializeComponent();
        Title = "Visita - Seleccionar cliente 1/2";
        BindingContext = _viewModel = new VisitaViewModel();

    }

	protected override async void OnAppearing()
	{
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
        await _viewModel.LoadsClientes();

    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }


}