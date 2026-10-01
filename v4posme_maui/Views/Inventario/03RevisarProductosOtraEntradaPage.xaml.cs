using v4posme_maui.ViewModels.Inventario;

namespace v4posme_maui.Views.Inventario;

public partial class RevisarProductosOtraEntradaPage : ContentPage
{
    public RevisarProductosOtraEntradaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is RevisarProductosOtraEntradaViewModel vm)
            vm.OnAppearing(Navigation);
    }

    private void OnValorChanged(object? sender, EventArgs e)
    {
        if (BindingContext is RevisarProductosInventarioBaseViewModel vm)
            vm.RecalcularCommand.Execute(null);
    }
}
