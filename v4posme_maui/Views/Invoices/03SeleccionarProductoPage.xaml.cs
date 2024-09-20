using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v4posme_maui.ViewModels.Invoices;

namespace v4posme_maui.Views.Invoices;

public partial class SeleccionarProductoPage : ContentPage
{
    public SeleccionarProductoPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((SeleccionarProductoViewModel)BindingContext).OnAppearing(Navigation);
    }
}