using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.Abonos;

namespace v4posme_maui.Views.Abonos;

public partial class CreditDetailInvoicePage : ContentPage
{
    private ToolbarItem _toolbarShareHistory;
    private CreditDetailInvoiceViewModel viewModel;
    private readonly IRepositoryParameters _repositoryParameters;
    public CreditDetailInvoicePage()
    {
        InitializeComponent();
        viewModel = (CreditDetailInvoiceViewModel)BindingContext;
        _repositoryParameters   = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _toolbarShareHistory = new ToolbarItem
        {
            Text            = "",
            IconImageSource = "receipt_long.svg",
            Command         = viewModel.ViewPaymentsCommand
        };
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ActualizarVisibilidad();
    }

    private async void ActualizarVisibilidad()
    {
        try
        {
            var shareHistoryParam   = await _repositoryParameters.PosMeFindByKey("MOBILE_SHOW_URL_CUSTOMER_PAY");
            if (shareHistoryParam is null) return;
            if (shareHistoryParam.Value.ToLowerInvariant().Contains("false")) return;
            var enableShareHistory      = !string.IsNullOrWhiteSpace(shareHistoryParam.Value);
            if (enableShareHistory)
            {
                if (!ToolbarItems.Contains(_toolbarShareHistory))
                    ToolbarItems.Add(_toolbarShareHistory);
            }
            else
                ToolbarItems.Remove(_toolbarShareHistory);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e.Message);
        }
        
    }
}