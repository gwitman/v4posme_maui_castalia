using CommunityToolkit.Maui.Core;
using DevExpress.Data.ODataLinq.Helpers;
using Plugin.BLE;
using SkiaSharp;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.More.ReporteVenta
{
	public class ReporteVentaViewModel : BaseViewModel
	{
		private readonly IRepositoryTbParameterSystem _parameterSystem;
		private readonly IRepositoryTbCustomer _repositoryTbCustomer;
		private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;

		public ICommand PrintCommand { get; }

		public ICommand HideFormCommand { get; }

		private TbCompany? _company;

		public TbCompany? Company
		{
			get => _company;
			private set => SetProperty(ref _company, value);
		}

		private ImageSource? _logoSource;

		public ImageSource? LogoSource
		{
			get => _logoSource;
			set => SetProperty(ref _logoSource, value);
		}

		private DateTime _fechaInical;
		private DateTime _fechaFinal;

		public DateTime FechaInical
		{
			get => _fechaInical;
			set
			{
				_fechaInical = value;
				SetProperty(ref _fechaInical, value);
			}
		}

		public DateTime FechaFinal
		{
			get => _fechaFinal;
			set
			{
				_fechaFinal = value;
				SetProperty(ref _fechaFinal, value);
			}
		}

		private bool _isFormVisible = false;
		private bool _isVisibleDate = true;

		public bool IsFormVisible
		{
			get => _isFormVisible;
			protected set => SetProperty(ref _isFormVisible, value);
		}

		public bool IsVisibleDate
		{
			get => _isVisibleDate;
			protected set => SetProperty(ref _isVisibleDate, value);
		}

		private string _totalFactura = "$ 0";

		public string TotalFactura
		{
			get => _totalFactura;
			protected set => SetProperty(ref _totalFactura, value);
		}

		private string _totalFacturaNIO = "$ 0";

		public string TotalFacturaNIO
		{
			get => _totalFacturaNIO;
			protected set => SetProperty(ref _totalFacturaNIO, value);
		}

		private string _totalCredito = "$ 0";

		public string TotalCredito
		{
			get => _totalCredito;
			protected set => SetProperty(ref _totalCredito, value);
		}

		private string _totalCreditoNIO = "$ 0";

		public string TotalCreditoNIO
		{
			get => _totalCreditoNIO;
			protected set => SetProperty(ref _totalCreditoNIO, value);
		}

		private double _totalFacturaDetalle = 0;

		public double TotalFacturaDetalle
		{
			get => _totalFacturaDetalle;
			protected set => SetProperty(ref _totalFacturaDetalle, value);
		}

		private double _invoicesHeight = 0;

		public double InvoicesHeight
		{
			get => _invoicesHeight;
			protected set => SetProperty(ref _invoicesHeight, value);
		}

		private double _invoicesHeight_2 = 0;

		public double InvoicesHeight_2
		{
			get => _invoicesHeight_2;
			protected set => SetProperty(ref _invoicesHeight_2, value);
		}

		private double _creditHeight = 0;

		public double CreditHeight
		{
			get => _creditHeight;
			protected set => SetProperty(ref _creditHeight, value);
		}

		private double _creditHeight_2 = 0;

		public double CreditHeight_2
		{
			get => _creditHeight_2;
			protected set => SetProperty(ref _creditHeight_2, value);
		}

		private double _visitHeight = 0;

		public double VisitHeight
		{
			get => _visitHeight;
			protected set => SetProperty(ref _visitHeight, value);
		}

		private string _totalUSD = "0";

		public string TotalUSD
		{
			get => _totalUSD;
			protected set => SetProperty(ref _totalUSD, value);
		}

		private string _totalNIO = "0";

		public string TotalNIO
		{
			get => _totalNIO;
			protected set => SetProperty(ref _totalNIO, value);
		}

		public ObservableCollection<ViewTempDtoReporteCierre> Invoices { get; }
		public ObservableCollection<ViewTempDtoReporteCierre> InvoicesNIO { get; }
		public ObservableCollection<ViewTempDtoReporteCierre> Credits { get; }
		public ObservableCollection<ViewTempDtoReporteCierre> CreditsNIO { get; }
		public ObservableCollection<ViewTempDtoReporteCierre> Visits { get; }

		public ReporteVentaViewModel()
		{
			Title = "Cierre";
			var today = DateTime.Today;

			_parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
			_repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
			_repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();

			PrintCommand = new Command(OnPrintCommand);
			HideFormCommand = new Command(OnHideFormCommand);
			
			FechaInical = new DateTime(today.Year, today.Month, 1);
			FechaFinal = new DateTime(today.Year, today.Month, DateTime.DaysInMonth(today.Year, today.Month));

			Invoices = [];
			Credits = [];
			Visits = [];
			InvoicesNIO = [];
			CreditsNIO = [];
		}

		public override async Task InitializeAsync(object parameter) => await OnAppearing(Navigation!);

		public async Task OnAppearing(INavigation navigation)
		{
			Navigation = navigation;

			await Task.Run(async () =>
			{
				var paramter = await _parameterSystem.PosMeFindLogo();
				var imageBytes = Convert.FromBase64String(paramter.Value!);
				LogoSource = ImageSource.FromStream(() => new MemoryStream(imageBytes));
				Company = VariablesGlobales.TbCompany;
			});

			IsBusy = false;
		}

		private async void OnHideFormCommand()
		{
			IsVisibleDate = false;
			IsFormVisible = true;

			await LoadReport();
		}

		public void BackForm()
		{
			IsVisibleDate = true;
			IsFormVisible = false;

			InvoicesHeight = 0;
			InvoicesHeight_2 = 0;
			CreditHeight = 0;
			CreditHeight_2 = 0;
			VisitHeight = 0;

			Invoices.Clear();
			Credits.Clear();
			Visits.Clear();
			InvoicesNIO.Clear();
			CreditsNIO.Clear();
		}

		private async Task LoadReport()
		{
			try
			{
				Invoices.Clear();
				Credits.Clear();
				Visits.Clear();
				InvoicesNIO.Clear();
				CreditsNIO.Clear();

				var _TotalFactura = 0m;
				var _TotalFacturaNIO = 0m;
				var _TotalCredito = 0m;
				var _TotalCreditoNIO = 0m;

				var totalNIO = 0m;
				var totalUSD = 0m;

				var transactions = (await _repositoryTbTransactionMaster.PosmeGetAll());

				var invoicesUS = transactions
					.Where(_invoices => _invoices.TransactionId == TypeTransaction.TransactionInvoiceBilling)
					.Where(_invoices => _invoices.TransactionOn >= FechaInical && _invoices.TransactionOn <= FechaFinal.AddHours(12))
					.Where(_invoices => _invoices.CurrencyId == TypeCurrency.Dolar)
					.Select(x => new ViewTempDtoReporteCierre()
					{
						DocumentNumber = x.TransactionNumber!,
						CurrencyId = (int)x.CurrencyId,
						CurrencyName = "$",
						Remaining = x.Amount,
					})
					.ToList();

				var creditsUS = transactions
					.Where(_credits => _credits.TransactionId == TypeTransaction.TransactionShare)
					.Where(_credits => _credits.TransactionOn >= FechaInical && _credits.TransactionOn <= FechaFinal.AddHours(12))
					.Where(_credits => _credits.CurrencyId == TypeCurrency.Dolar)
					.Select(x => new ViewTempDtoReporteCierre()
					{
						DocumentNumber = x.TransactionNumber!,
						CurrencyId = (int)x.CurrencyId,
						CurrencyName = "$",
						Remaining = x.Amount,
					})
					.ToList();

				var invoicesNIO = transactions
					.Where(_invoices => _invoices.TransactionId == TypeTransaction.TransactionInvoiceBilling)
					.Where(_invoices => _invoices.TransactionOn >= FechaInical && _invoices.TransactionOn <= FechaFinal.AddHours(12))
					.Where(_invoices => _invoices.CurrencyId == TypeCurrency.Cordoba)
					.Select(x => new ViewTempDtoReporteCierre()
					{
						DocumentNumber = x.TransactionNumber!,
						CurrencyId = (int)x.CurrencyId,
						CurrencyName = "C$",
						Remaining = x.Amount,
					})
					.ToList();

				var creditsNIO = transactions
					.Where(_credits => _credits.TransactionId == TypeTransaction.TransactionShare)
					.Where(_credits => _credits.TransactionOn >= FechaInical && _credits.TransactionOn <= FechaFinal.AddHours(12))
					.Where(_credits => _credits.CurrencyId == TypeCurrency.Cordoba)
					.Select(x => new ViewTempDtoReporteCierre()
					{
						DocumentNumber = x.TransactionNumber!,
						CurrencyId = (int)x.CurrencyId,
						CurrencyName = "C$",
						Remaining = x.Amount,
					})
					.ToList();

				var visits = transactions
					.Where(_visits => _visits.TransactionId == TypeTransaction.TransactionQueryMedical)
					.Where(_visits => _visits.TransactionOn >= FechaInical && _visits.TransactionOn <= FechaFinal.AddHours(12))
					.Select(x => new ViewTempDtoReporteCierre()
					{
						DocumentNumber = x.TransactionNumber!,
						Remaining = x.Amount,
						Date = x.TransactionOn.ToString("dd/MM/yyyy"),
						ClientId = x.EntityId
					})
					.ToList();

				foreach (var item in invoicesUS)
				{
					Invoices.Add(item);
					_TotalFactura += item.Remaining;
					totalUSD += item.Remaining;
				}

				foreach (var item in invoicesNIO)
				{
					InvoicesNIO.Add(item);
					_TotalFacturaNIO += item.Remaining;
					totalNIO += item.Remaining;
				}

				foreach (var item in creditsUS)
				{
					Credits.Add(item);
					_TotalCredito += item.Remaining;
					totalUSD += item.Remaining;
				}

				foreach (var item in creditsNIO)
				{
					CreditsNIO.Add(item);
					_TotalCreditoNIO += item.Remaining;
					totalNIO += item.Remaining;
				}

				foreach (var item in visits)
				{
					var customer = await _repositoryTbCustomer.PosMeFindEntityId(item.ClientId);
					
					if (customer is not null)
					{
						item.ClientName = customer.FirstName + " " + customer.LastName;
					}else
					{
						item.ClientName = "-";
					}

					Visits.Add(item);
				}

				IsBusy = false;
				InvoicesHeight = 24 * Invoices.Count;
				InvoicesHeight_2 = 24 * InvoicesNIO.Count;
				CreditHeight = 24 * Credits.Count;
				CreditHeight_2 = 24 * CreditsNIO.Count;
				VisitHeight = 24 * Visits.Count;
				TotalNIO = $"C$ {totalNIO:N2}";
				TotalUSD = $"$ {totalUSD:N2}";

				TotalFactura = $"$ {_TotalFactura:N2}";
				TotalFacturaNIO = $"C$ {_TotalFacturaNIO:N2}";
				TotalCredito = $"$ {_TotalCredito:N2}";
				TotalCreditoNIO = $"C$ {_TotalCreditoNIO:N2}";
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				IsBusy = false;
				IsVisibleDate = !IsVisibleDate;
				IsFormVisible = !IsFormVisible;
			}
		}

		private async void OnPrintCommand(object obj)
		{
			var parametroPrinter = await _parameterSystem.PosMeFindPrinter();
			var logo = await _parameterSystem.PosMeFindLogo();
			
			if (string.IsNullOrWhiteSpace(parametroPrinter.Value))
			{
				return;
			}

			var printer = new Printer(parametroPrinter.Value);

			if (!CrossBluetoothLE.Current.IsOn)
			{
				ShowToast(Mensajes.MensajeBluetoothState, ToastDuration.Long, 18);
				return;
			}

			IsBusy = true;

			if (!string.IsNullOrWhiteSpace(logo.Value))
			{
				var readImage = Convert.FromBase64String(logo.Value!);
				printer.AlignRight();
				printer.Image(SKBitmap.Decode(readImage));
			}

			printer.AlignCenter();
			printer.BoldMode(Company!.Name!);
			printer.BoldMode($"CIERRE");
			printer.NewLine();
			printer.AlignCenter();
			printer.BoldMode($"DEL {FechaInical:dd/MM/yyyy} AL {FechaFinal:dd/MM/yyyy}");
			printer.NewLine();
			printer.AlignLeft();

			printer.Append($"Facturas de contado C$");
			printer.NewLine();

			var totalInvoicesNIO = 0.0m;
			foreach (var item in InvoicesNIO)
			{
				totalInvoicesNIO += item.Remaining;
				printer.Append($"{item.DocumentNumber}     {item.Remaining:N2}");
				printer.NewLine();
			}

			printer.Append($"Total:         {totalInvoicesNIO:N2}");
			printer.NewLine();

			printer.Append($"Abonos C$");
			printer.NewLine();

			var totalAbonosNIO = 0.0m;
			foreach (var item in CreditsNIO)
			{
				totalAbonosNIO += item.Remaining;
				printer.Append($"{item.DocumentNumber}     {item.Remaining:N2}");
				printer.NewLine();
			}

			printer.Append($"Total:         {totalAbonosNIO:N2}");
			printer.NewLine();

			printer.Append($"Facturas de contado $");
			printer.NewLine();

			var totalInvoicesUSD = 0.0m;
			foreach (var item in InvoicesNIO)
			{
				totalInvoicesUSD += item.Remaining;
				printer.Append($"{item.DocumentNumber}     {item.Remaining:N2}");
				printer.NewLine();
			}

			printer.Append($"Total:         {totalInvoicesUSD:N2}");
			printer.NewLine();

			printer.Append($"Abonos $");
			printer.NewLine();

			var totalAbonosUSD = 0.0m;
			foreach (var item in CreditsNIO)
			{
				totalAbonosUSD += item.Remaining;
				printer.Append($"{item.DocumentNumber}     {item.Remaining:N2}");
				printer.NewLine();
			}

			printer.Append($"Total:         {totalAbonosUSD:N2}");
			printer.NewLine();
			printer.NewLine();
			printer.Append("--------------------------------------");
			printer.NewLine();
			printer.NewLine();

			printer.Append($"Visitas");
			printer.NewLine();

			foreach (var item in Visits)
			{
				var customer = await _repositoryTbCustomer.PosMeFindEntityId(item.ClientId);

				if (customer is not null)
				{
					item.ClientName = customer.FirstName + " " + customer.LastName;
				}
				else
				{
					item.ClientName = "-";
				}

				printer.Append($"{item.ClientName}     {item.Date:dd/MM/yyyy}");
				printer.NewLine();
			}

			printer.FullPaperCut();
			printer.Print();

			if (printer.Device is null)
			{
				ShowToast(Mensajes.MensajeDispositivoNoConectado, ToastDuration.Long, 18);
			}

			IsBusy = false;
		}
	}
}
