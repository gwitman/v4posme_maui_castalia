using System.Collections.ObjectModel;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using System.Diagnostics;
using Unity;

namespace v4posme_maui.ViewModels.More.Visita
{
	public class VisitaFormViewModel : BaseViewModel
	{

		private DateTime _selectedDate = DateTime.Now.AddDays(1);

		private ObservableCollection<DtoCatalogItem>? _tipifaciones;

		private string _comentario = string.Empty;

		public DtoCatalogItem? SelectedTipificacion { get; set; }

		public DateTime SelectedDate
		{
			get => _selectedDate;
			set
			{
				_selectedDate = value;
				SetProperty(ref _selectedDate, value);
			}
		}

		public ObservableCollection<DtoCatalogItem>? Tipificaciones
		{
			get => _tipifaciones;
			set => SetProperty(ref _tipifaciones, value);
		}

		public string Comentario
		{
			get => _comentario;
			set => SetProperty(ref _comentario, value);
		}

		public void OnAppearing(INavigation? navigation)
		{
			Navigation = navigation;
		}

		public ViewTempDtoVisita CurrentVisita { get; set; }

		private readonly IRepositoryTbTransactionMaster _repositoryTransactionMaster;
		private readonly HelperCore _helper;
		private Api_AppMobileApi_GetDataDownloadCustomerResponse _customerResponse;

		public VisitaFormViewModel()
		{
			SelectedDate = DateTime.Now.AddDays(1);

			Tipificaciones =
			[
				new DtoCatalogItem((int)TypeQueryMedical.ConsultaMedica, "Consulta Médica", ""),
				new DtoCatalogItem((int)TypeQueryMedical.Salida, "Salida", ""),
				new DtoCatalogItem((int)TypeQueryMedical.Entrada, "Entrada", ""),
				new DtoCatalogItem((int)TypeQueryMedical.Visita, "Visita", ""),
			];

			if (Tipificaciones.Any())
			{
				SelectedTipificacion = Tipificaciones.First();
			}
			
			CurrentVisita = new();

			IsBusy = false;
			
			_customerResponse = new();
			
			_helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
			
			_repositoryTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
		}

		public async Task<bool> OnAplicarVisita(object? obj)
		{
			try
			{
				IsBusy = true;
				var codigoVisita = await _helper.GetCodigoVisita();

				_customerResponse = CurrentVisita.Customer;

				var transactionMaster = new TbTransactionMaster
				{
					TransactionId = TypeTransaction.TransactionQueryMedical,
					SubAmount = 0,
					Discount = 0,
					Amount = 0,
					Comment = Comentario,
					TransactionNumber = codigoVisita,
					TransactionOn = SelectedDate,
					EntitySecondaryId = _customerResponse.CustomerNumber,
					EntityId = _customerResponse.EntityId,
					CurrencyId = TypeCurrency.Cordoba,
					Reference1 = "",
					CustomerCreditLineId = 0,
					CustomerIdentification = _customerResponse.Identification!
				};

				var taskTransactionMaster = _repositoryTransactionMaster.PosMeInsert(transactionMaster);
				var taskPlus = _helper.PlusCounter();
				await Task.WhenAll([taskPlus, taskTransactionMaster]);
				IsBusy = false;

				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				return false;
			}
		}
	}
}
