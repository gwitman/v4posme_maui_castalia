using System.Collections.ObjectModel;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using System.Diagnostics;
using Unity;
using System.Web;
using DevExpress.Maui.DataForm;
using v4posme_maui.Views;
using Android.Content.Res;

namespace v4posme_maui.ViewModels.More.Visita
{
	public class VisitaFormViewModel : BaseViewModel, IQueryAttributable
    {
        private IRepositoryTbCustomer _repositoryTbCustomer;
        private readonly IRepositoryTbTransactionMaster _repositoryTransactionMaster;
        private readonly HelperCore _helper;
        public ViewTempDtoVisita CurrentVisita { get; set; }

        public bool ErrorCurrency { get; set; }        

        private DateTime _selectedDate = DateTime.Now.AddDays(1);
        public DateTime SelectedDate
        {
			get => _selectedDate;
			set
			{
				_selectedDate = value;
				SetProperty(ref _selectedDate, value);
			}
		}

        public DtoCatalogItem? SelectedTipificacion { get; set; }

        private ObservableCollection<DtoCatalogItem>? _tipificaciones;
        public ObservableCollection<DtoCatalogItem>? Tipificaciones
		{
			get => _tipificaciones;
			set => SetProperty(ref _tipificaciones, value);
		}

        private string _comentario = string.Empty;
        public string Comentario
		{
			get => _comentario;
			set => SetProperty(ref _comentario, value);
		}


        public VisitaFormViewModel()
        {
            CurrentVisita					= new();
            _repositoryTbCustomer			= VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
            _helper							= VariablesGlobales.UnityContainer.Resolve<HelperCore>();
            _repositoryTransactionMaster	= VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
            Title			= "Agendar visita 2/2";         
            LoadComboBox();          
            
        }

       

        public void OnAppearing(INavigation? navigation)
		{
			Navigation = navigation;
		}      

        public async void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            var id = HttpUtility.UrlDecode(query["id"] as string);
            await LoadVisita(id);
        }


        private async Task LoadVisita(string? param)
        {
            var customer			= await _repositoryTbCustomer.PosMeFindEntityId(Convert.ToInt32(param!));
            CurrentVisita.Customer	= customer;
            LoadComboBox();
            IsBusy = false;
        }

        private void LoadComboBox()
        {
            SelectedDate	= DateTime.Now.AddDays(1);
            Tipificaciones	=
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

        }


        public async Task<bool> OnAplicarVisita(object? obj)
		{
			try
			{

                if (string.IsNullOrWhiteSpace(CurrentVisita.Comentario))
                {
                    throw new Exception(Mensajes.MessageCommentEmpty);
                }

                if (CurrentVisita.FechaVisita <= DateTime.Now )
                {
                    throw new Exception(Mensajes.MessageDateMoreThan);
                }

                if (string.IsNullOrWhiteSpace(CurrentVisita.Tipificacion))
                {
                    throw new Exception(Mensajes.MessageVisitTipificationEmpty);
                }


                IsBusy                  = true;
				var codigoVisita        = await _helper.GetCodigoVisita();
				var _customerResponse   = CurrentVisita.Customer;

				var transactionMaster = new TbTransactionMaster
				{
					TransactionId           = TypeTransaction.TransactionQueryMedical,
					SubAmount               = 0,
					Discount                = 0,
					Amount                  = 0,
					Comment                 = Comentario,
					TransactionNumber       = codigoVisita,
					TransactionOn           = SelectedDate,
					EntitySecondaryId       = _customerResponse.CustomerNumber,
					EntityId                = _customerResponse.EntityId,
					CurrencyId              = TypeCurrency.Cordoba,
					Reference1              = CurrentVisita.Tipificacion,
                    CustomerCreditLineId    = 0,
					CustomerIdentification  = _customerResponse.Identification!
                };

				var taskTransactionMaster   = _repositoryTransactionMaster.PosMeInsert(transactionMaster);
				var taskPlus                = _helper.PlusCounter();
				await Task.WhenAll([taskPlus, taskTransactionMaster]);

				IsBusy                  = false;
                Mensaje                 = Mensajes.MessageOkVisita;
                PopupBackgroundColor    = Colors.Green;                
                PopUpShow               = true;
                return true;
			}
			catch (Exception e)
			{
                Debug.WriteLine(e);
                Mensaje                 = Mensajes.MessageErrorVisita;
                PopupBackgroundColor    = Colors.Red;
                PopUpShow               = true;                
                IsBusy                  = false;
                return false;
			}
		}
	}
}
