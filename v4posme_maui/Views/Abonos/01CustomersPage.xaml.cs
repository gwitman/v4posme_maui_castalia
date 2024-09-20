using v4posme_maui.ViewModels.Abonos;

namespace v4posme_maui.Views.Abonos;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AbonosPage : ContentPage
{
    private readonly AbonosViewModel _viewModel;

    public AbonosPage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new AbonosViewModel();
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