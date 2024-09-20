using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using v4posme_maui.ViewModels.Upload;

namespace v4posme_maui.Views.Upload;

public partial class UploadPage : ContentPage
{
    public UploadPage()
    {
        InitializeComponent();
    }
    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }

    protected override void OnAppearing()
    {
        ((UploadViewModel)BindingContext).OnAppearing(Navigation);
    }
}