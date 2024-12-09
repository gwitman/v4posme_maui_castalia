using v4posme_maui.Models;using DevExpress.Maui.DataForm;using v4posme_maui.Services.Repository;using v4posme_maui.Services.SystemNames;using v4posme_maui.ViewModels.More.Visita;using Unity;using DevExpress.Maui.Core;using System.ComponentModel;using v4posme_maui.Services;using v4posme_maui.ViewModels.Abonos;using v4posme_maui.ViewModels;using v4posme_maui.Views.Abonos;using v4posme_maui.Views.Invoices;namespace v4posme_maui.Views.More.Visita;public partial class VisitaFormPage : ContentPage, INotifyPropertyChanged{
	private readonly IRepositoryTbTransactionMaster repositoryTbTransactionMaster;
	private static IRepositoryTbCustomer RepositoryTbCustomer => VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();

	private DetailEditFormViewModel ViewModel => (DetailEditFormViewModel)BindingContext;
	private Api_AppMobileApi_GetDataDownloadCustomerResponse _customer;
	public DtoVisita CurrentVisita { get; set; }
	public VisitaFormViewModel VisitaFormViewModel { get; set; }

	public VisitaFormPage()
	{
		Title = "Visita - Formulario";
		InitializeComponent();
		repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
		VisitaFormViewModel = new VisitaFormViewModel();
		TxtFecha.BindingContext = VisitaFormViewModel;
		TxtTipificacion.BindingContext = VisitaFormViewModel;
		TextComentario.BindingContext = VisitaFormViewModel;
		TxtTipificacion.SelectedItem = VisitaFormViewModel.SelectedTipificacion;
	}

	protected override async void OnAppearing()
	{
		base.OnAppearing();
		_customer = (Api_AppMobileApi_GetDataDownloadCustomerResponse)DataForm.DataObject;
		var customer = await RepositoryTbCustomer.PosMeFindCustomer(_customer.CustomerNumber!);
		CurrentVisita = new DtoVisita()
		{
			Customer = customer,
		};

		DataForm.CommitMode = CommitMode.LostFocus;
		VisitaFormViewModel.OnAppearing(Navigation);
	}

	private async void SaveItemClick(object? sender, EventArgs e)
	{
		if (string.IsNullOrWhiteSpace(TextComentario.Text))
		{
			await DisplayAlert("", Mensajes.MessageCommentEmpty, "OK");
			return;
		}

		if (TxtFecha.Date <= DateTime.Now || TxtFecha.Date is null)
		{
			await DisplayAlert("", Mensajes.MessageDateMoreThan, "OK");
			return;
		}

		CurrentVisita.Comentario = TextComentario.Text;
		CurrentVisita.FechaVisita = TxtFecha.Date!.Value;
		CurrentVisita.Tipificacion = (TxtTipificacion.SelectedItem! as DtoCatalogItem)!.Key.ToString()!;
		VisitaFormViewModel.CurrentVisita = CurrentVisita;

		try
		{
			var response = await VisitaFormViewModel.OnAplicarAbono(sender);

			if (response)
			{
				await DisplayAlert("", Mensajes.MessageOkVisita, "OK");
			}
			else
			{
				await DisplayAlert("", Mensajes.MessageErrorVisita, "OK");
				return;
			}

			Application.Current!.MainPage = new MainPage();
			await Navigation.PopToRootAsync();
		}
		catch (Exception ex)
		{
			if (ex is not ArgumentException)
			{
				await DisplayAlert("", ex.Message, "OK");
			}
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

	protected override void OnDisappearing()
	{

	}}