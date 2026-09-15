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

        // Si se dejaron paginas apiladas encima del dashboard (por ejemplo la
        // visualizacion de una compra/salida abierta desde el listado) y el usuario
        // regreso a Impresiones desde el menu lateral, se limpian para volver a mostrar
        // los tabs y no la ultima visualizacion.
        var stack = Navigation.NavigationStack.ToArray();
        for (var i = stack.Length - 1; i > 0; i--)
        {
            if (stack[i] is not null && stack[i] is not DashboardPrinterPage)
                Navigation.RemovePage(stack[i]);
        }

        _viewModel.OnAppearing(Navigation);
    }
}