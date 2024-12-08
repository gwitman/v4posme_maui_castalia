using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v4posme_maui.ViewModels.Invoices;

namespace v4posme_maui.Views.Invoices;

public partial class RevisarProductosSeleccionadosPage : ContentPage
{
    public RevisarProductosSeleccionadosPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((RevisarProductosSeleccionadosViewModel)BindingContext).OnAppearing(Navigation);
    }
}