using v4posme_maui.ViewModels.More.Visita;
using v4posme_maui.Views.Abonos;
using v4posme_maui.Views.Invoices;

namespace v4posme_maui.Views.More.Visita;

public partial class VisitaPage : ContentPage
{
    private readonly VisitaViewModel visitaViewModel;

    public VisitaPage()
    {
        InitializeComponent();
        Title = "Visita - Seleccionar cliente";
        visitaViewModel = (VisitaViewModel)BindingContext;
    }

	protected override void OnAppearing()
	{
        visitaViewModel.OnAppearing(Navigation);
	}
}