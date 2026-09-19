using v4posme_maui.ViewModels.More.CashInflow;

namespace v4posme_maui.Views.More.CashInflow;

public partial class CashInflowPage : ContentPage
{
    private readonly CashInflowViewModel _viewModel;

    public CashInflowPage()
    {
        InitializeComponent();
        _viewModel = (CashInflowViewModel)BindingContext;
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
