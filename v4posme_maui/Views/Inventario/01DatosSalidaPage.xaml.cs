using v4posme_maui.ViewModels.Inventario;

namespace v4posme_maui.Views.Inventario;

public partial class DatosSalidaPage : ContentPage
{
    public DatosSalidaPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        if (BindingContext is DatosSalidaViewModel vm)
            vm.OnAppearing(Navigation);
    }
}
