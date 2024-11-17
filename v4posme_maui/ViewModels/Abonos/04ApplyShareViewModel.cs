using System.Diagnostics;
using System.Web;
using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;

namespace v4posme_maui.ViewModels.Abonos;

public class AplicarAbonoViewModel : BaseViewModel, IQueryAttributable
{
	private readonly IRepositoryDocumentCredit _repositoryDocumentCredit;
	private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization;
	private readonly IRepositoryTbCustomer _repositoryTbCustomer;
	private readonly IRepositoryTbTransactionMaster _repositoryTransactionMaster;
	private readonly IRepositoryParameters _repositoryParameters;
	private readonly HelperCore _helper;
	private readonly HelperCustomerCreditDocumentAmortization _helperCustomerCreditDocumentAmortization;
	private Api_AppMobileApi_GetDataDownloadDocumentCreditResponse _documentCreditResponse;
	private Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse _documentCreditAmortization;
	private Api_AppMobileApi_GetDataDownloadCustomerResponse _customerResponse;

	public AplicarAbonoViewModel()
	{
		_documentCreditResponse = new();
		_documentCreditAmortization = new();
		_customerResponse = new();
		_helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
		_helperCustomerCreditDocumentAmortization = VariablesGlobales.UnityContainer.Resolve<HelperCustomerCreditDocumentAmortization>();
		Title = "Completar Abono 4/5";
		_repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
		_repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
		_repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
		_repositoryTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
		_repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
		AplicarAbonoCommand = new Command(OnAplicarAbono, OnValidateMonto);
		ClearMontoCommand = new Command(OnClearMontoCommand);
		PropertyChanged += (_, _) => AplicarAbonoCommand.ChangeCanExecute();
	}

	private void OnClearMontoCommand(object obj)
	{
		Monto = decimal.Zero;
	}

	private bool OnValidateMonto(object arg)
	{
		return !Validate();
	}

	private async void OnAplicarAbono(object obj)
	{
		if (Validate())
		{
			return;
		}

		try
		{
			IsBusy = true;
			var codigoAbono = await _helper.GetCodigoAbono();
			//Obtener Cliente
			_customerResponse = await _repositoryTbCustomer.PosMeFindCustomer(DocumentCreditAmortizationResponse.CustomerNumber!);

			//para mostrar los saldo final e inicial
			var mostrarPrintSinSaldos = await _helper.GetValueParameter("CXC_SHOW_BALANCE_IN_SHARE_MOBILE","false");

			VariablesGlobales.DtoAplicarAbono = new ViewTempDtoAbono(
				codigoAbono,
				_customerResponse.EntityId,
				_customerResponse.FirstName!,
				_customerResponse.LastName!,
				_customerResponse.Identification!,
				DateTime.Now,
				DocumentCreditAmortizationResponse.DocumentNumber!,
				CurrencyName!,
				Monto,
				SaldoInicial,
				SaldoFinal,
				Description!
			);

			//Aplicar Abono
			string reference = await HelperCustomerCreditDocumentAmortization.ApplyShare(_customerResponse.EntityId, DocumentCreditResponse.DocumentNumber!, Monto);

			//Ingrear Abono 
			var tmpMonto = Monto;
			var transactionMaster = new TbTransactionMaster
			{
				TransactionId = TypeTransaction.TransactionShare,
				SubAmount = Monto,
				Discount = SaldoInicial,
				Amount = SaldoFinal,
				Comment = Description,
				TransactionNumber = codigoAbono,
				TransactionOn = DateTime.Now,
				EntitySecondaryId = _customerResponse.CustomerNumber,
				EntityId = _customerResponse.EntityId,
				CurrencyId = (TypeCurrency)CurrencyId,
				Reference1 = reference,
				CustomerCreditLineId = _customerResponse.CustomerCreditLineId,
				CustomerIdentification = _customerResponse.Identification!
			};
			var taskTransactionMaster = _repositoryTransactionMaster.PosMeInsert(transactionMaster);
			var taskPlus = _helper.PlusCounter();
			await Task.WhenAll([taskPlus, taskTransactionMaster]);

			if (mostrarPrintSinSaldos == "true")
			{
				await NavigationService.NavigateToAsync<ValidarAbonoViewModel>();
			}
			else
			{
				await NavigationService.NavigateToAsync<ValidarAbonoHideSaldoViewModel>();
			}
			
			IsBusy = false;
		}
		catch (Exception e)
		{
			Debug.WriteLine(e);
		}
	}

	private bool Validate()
	{
		if (string.IsNullOrWhiteSpace(Description))
		{
			//ShowToast("Especifique una descripción del abono", ToastDuration.Long, 16);
			return true;
		}

		if (decimal.Compare(Monto, decimal.Zero) <= 0)
		{
			//ShowToast("Especifique un monto del abono", ToastDuration.Long, 16);
			return true;
		}

		if (decimal.Compare(SaldoFinal, decimal.Zero) >= 0) return false;
		ShowToast(Mensajes.MensajeSaldoNegativo, ToastDuration.Long, 16);
		return true;
	}



	public Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse DocumentCreditAmortizationResponse
	{
		get => _documentCreditAmortization;
		set => SetProperty(ref _documentCreditAmortization, value);
	}

	public Api_AppMobileApi_GetDataDownloadDocumentCreditResponse DocumentCreditResponse
	{
		get => _documentCreditResponse;
		set => SetProperty(ref _documentCreditResponse, value);
	}

	public bool MontoError { get; set; }

	public bool DescriptionError { get; set; }

	private string? _currencyName;

	public string? CurrencyName
	{
		get => _currencyName;
		set => SetProperty(ref _currencyName, value);
	}

	private string? _description;

	public string? Description
	{
		get => _description;
		set => SetProperty(ref _description, value);
	}

	private decimal _saldoInicial;

	public decimal SaldoInicial
	{
		get => _saldoInicial;
		set => SetProperty(ref _saldoInicial, value);
	}

	private decimal _monto;

	public decimal Monto
	{
		get => _monto;
		set
		{
			SetProperty(ref _monto, value);
			_saldoFinal = decimal.Subtract(SaldoInicial, value);
			OnPropertyChanged(nameof(SaldoFinal));
		}
	}

	private decimal _saldoFinal;

	public decimal SaldoFinal
	{
		get => _saldoFinal;
		set => SetProperty(ref _saldoFinal, value);
	}

	public Command AplicarAbonoCommand { get; }

	public override async Task InitializeAsync(object parameter)
	{
		await LoadInvoices(parameter as string);
	}

	private async Task LoadInvoices(string? parameter)
	{
		if (string.IsNullOrWhiteSpace(parameter))
		{
			return;
		}

		DocumentCreditResponse = await _repositoryDocumentCredit.PosMeFindDocumentNumber(parameter);
		DocumentCreditAmortizationResponse = await _repositoryDocumentCreditAmortization.PosMeFindByDocumentNumber(parameter);
		CurrencyId = DocumentCreditResponse.CurrencyId;
		CurrencyName = DocumentCreditResponse.CurrencyName!;
		SaldoInicial = DocumentCreditResponse.Remaining;
		IsBusy = false;
	}

	public int CurrencyId { get; set; }
	public Command ClearMontoCommand { get; }

	public async void ApplyQueryAttributes(IDictionary<string, object> query)
	{
		var id = HttpUtility.UrlDecode(query["id"] as string);
		await LoadInvoices(id);
	}

	public void OnAppearing(INavigation navigation)
	{
		Navigation = navigation;
	}
}