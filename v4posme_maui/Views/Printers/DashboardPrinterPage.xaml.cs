using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v4posme_maui.ViewModels.Printers;

namespace v4posme_maui.Views.Printers;

public partial class DashboardPrinterPage : ContentPage
{
    private readonly DashboardPrinterViewModel _viewModel;

    public DashboardPrinterPage()
    {
        InitializeComponent();
        _viewModel = (DashboardPrinterViewModel)BindingContext;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
    }
}