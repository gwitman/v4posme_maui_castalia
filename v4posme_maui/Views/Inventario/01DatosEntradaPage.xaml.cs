using v4posme_maui.ViewModels.Inventario;

namespace v4posme_maui.Views.Inventario;

public partial class DatosEntradaPage : ContentPage
{
    public DatosEntradaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DatosEntradaViewModel vm)
            vm.OnAppearing(Navigation);
    }
}
