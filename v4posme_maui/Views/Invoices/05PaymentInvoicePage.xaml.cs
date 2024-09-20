using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v4posme_maui.ViewModels.Invoices;

namespace v4posme_maui.Views.Invoices;

public partial class PaymentInvoicePage : ContentPage
{
    public PaymentInvoicePage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((PaymentInvoiceViewModel)BindingContext).OnAppearing(Navigation);
    }
}