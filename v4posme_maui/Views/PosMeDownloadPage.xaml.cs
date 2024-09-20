using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.ViewModels;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.SystemNames;
namespace v4posme_maui.Views;

public partial class DownloadPage : ContentPage
{
    private readonly RestApiAppMobileApi _restApiDownload;
    public DownloadPage()
    {
        _restApiDownload = new RestApiAppMobileApi();
        InitializeComponent();
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }

    protected override void OnAppearing()
    {
        ((PosMeDownloadViewModel)BindingContext).OnAppearing(Navigation);
    }
}