using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Maui.DataForm;
using v4posme_maui.ViewModels.Invoices;

namespace v4posme_maui.Views.Invoices;

public partial class DataInvoicesPage : ContentPage
{
    public DataInvoicesPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((DataInvoicesViewModel)BindingContext).OnAppearing(Navigation);
    }
}