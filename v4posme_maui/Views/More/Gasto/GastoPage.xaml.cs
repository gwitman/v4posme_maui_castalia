using v4posme_maui.ViewModels.More.Gasto;

namespace v4posme_maui.Views.More.Gasto;

public partial class GastoPage : ContentPage
{
    private readonly GastoViewModel _viewModel;

    public GastoPage()
    {
        InitializeComponent();
        _viewModel = (GastoViewModel)BindingContext;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
    }

    private async void BackToHome_OnClicked(object? sender, EventArgs e)
    {
        Application.Current!.MainPage = new MainPage();
        await Navigation.PopToRootAsync();
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        _viewModel.PopUpShow = false;
    }
}
