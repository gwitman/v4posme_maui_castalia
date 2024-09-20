using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.ViewModels;
using v4posme_maui.ViewModels.Abonos;
using Unity;

namespace v4posme_maui.Views.Abonos;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CustomerDetailInvoicePage : ContentPage
{
    private readonly CustomerDetailInvoiceViewModel _viewModel;

    public CustomerDetailInvoicePage()
    {
        InitializeComponent();
        BindingContext = _viewModel = new CustomerDetailInvoiceViewModel();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _viewModel.OnAppearing(Navigation);
    }
    }