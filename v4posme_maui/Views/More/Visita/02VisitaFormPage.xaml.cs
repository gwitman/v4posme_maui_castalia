using v4posme_maui.Models;
using DevExpress.Maui.DataForm;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.ViewModels.More.Visita;
using DevExpress.Maui.Editors;
namespace v4posme_maui.Views.More.Visita;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class VisitaFormPage : ContentPage
{
    private readonly VisitaFormViewModel VisitaFormViewModel;

	public VisitaFormPage()
	{		
		InitializeComponent();
        BindingContext = VisitaFormViewModel = new VisitaFormViewModel();
    }

	protected override void OnAppearing()
	{
        base.OnAppearing();
        VisitaFormViewModel.OnAppearing(Navigation);

    }

    

    private void TxtComentario_OnTextChanged(object? sender, EventArgs e)
    {
        var text = sender as TextEdit;
        if (string.IsNullOrWhiteSpace(text!.Text))
        {
            TextComentario.HasError = true;
        }
    }


    private void DataForm_OnValidateForm(object sender, DataFormValidationEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(TextComentario.Text))
        {
            e.HasErrors = true;
            TextComentario.HasError = true;
        }

        if (TxtFecha.Date <= DateTime.Now || TxtFecha.Date is null)
        {
            e.HasErrors = true;
            TxtFecha.HasError = true;
        }
    }

    private void ClosePopup_Clicked(object? sender, EventArgs e)
    {
        Popup.IsOpen = false;
    }

    private async void SaveItemClick(object? sender, EventArgs e)
    {        
        VisitaFormViewModel.CurrentVisita.Comentario    = TextComentario.Text is null ? "" : TextComentario.Text;
        VisitaFormViewModel.CurrentVisita.FechaVisita   = TxtFecha.Date is null ? DateTime.Now : TxtFecha.Date!.Value;
        VisitaFormViewModel.CurrentVisita.Tipificacion  = TxtTipificacion.SelectedItem is null ? "" : (TxtTipificacion.SelectedItem! as DtoCatalogItem)!.Key.ToString()!;        
        var response = await VisitaFormViewModel.OnAplicarVisita(sender);
        if (response)
        {
            Application.Current!.MainPage = new MainPage();
            await Navigation.PopToRootAsync();
        }          
        
    }



}