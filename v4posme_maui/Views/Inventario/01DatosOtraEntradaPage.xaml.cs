using v4posme_maui.ViewModels.Inventario;

namespace v4posme_maui.Views.Inventario;

public partial class DatosOtraEntradaPage : ContentPage
{
    public DatosOtraEntradaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DatosOtraEntradaViewModel vm)
            vm.OnAppearing(Navigation);
    }
}
