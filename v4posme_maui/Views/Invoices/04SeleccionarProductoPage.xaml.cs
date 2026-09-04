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

    // La seleccion de producto (4/6) es la pantalla base del flujo de facturacion rapida.
    // Se bloquea el boton atras del dispositivo para que no se regrese a la lista de
    // clientes (1/6); las pantallas de cliente, datos de factura y credito solo se
    // alcanzan desde el menu desplegable de esta pantalla.
    protected override bool OnBackButtonPressed()
    {
        return true;
    }
}