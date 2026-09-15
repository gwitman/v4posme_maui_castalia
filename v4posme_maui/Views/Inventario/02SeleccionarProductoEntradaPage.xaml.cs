using v4posme_maui.ViewModels.Inventario;

namespace v4posme_maui.Views.Inventario;

public partial class SeleccionarProductoEntradaPage : ContentPage
{
    public SeleccionarProductoEntradaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SeleccionarProductoEntradaViewModel vm)
            vm.OnAppearing(Navigation);
    }
}
