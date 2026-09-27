using v4posme_maui.ViewModels.More.CashOutflow;

namespace v4posme_maui.Views.More.CashOutflow;

public partial class CashOutflowPage : ContentPage
{
    private readonly CashOutflowViewModel _viewModel;

    public CashOutflowPage()
    {
        InitializeComponent();
        _viewModel = (CashOutflowViewModel)BindingContext;
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
