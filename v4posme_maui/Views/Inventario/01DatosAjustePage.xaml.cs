using v4posme_maui.ViewModels.Inventario;

namespace v4posme_maui.Views.Inventario;

public partial class DatosAjustePage : ContentPage
{
    public DatosAjustePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DatosAjusteViewModel vm)
            vm.OnAppearing(Navigation);
    }
}
