using v4posme_maui.ViewModels.Inventario;

namespace v4posme_maui.Views.Inventario;

public partial class SeleccionarProductoOtraEntradaPage : ContentPage
{
    public SeleccionarProductoOtraEntradaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is SeleccionarProductoOtraEntradaViewModel vm)
            vm.OnAppearing(Navigation);
    }

    private void OnValorChanged(object? sender, EventArgs e)
    {
        if (BindingContext is SeleccionarProductoInventarioBaseViewModel vm)
            vm.RecalcularCommand.Execute(null);
    }
}
