using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Maui.DataForm;
using v4posme_maui.ViewModels.Invoices;

namespace v4posme_maui.Views.Invoices;


public partial class DataInvoiceCreditPage : ContentPage
{
	public DataInvoiceCreditPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        ((DataInvoiceCreditViewModel)BindingContext).OnAppearing(Navigation);
    }

}