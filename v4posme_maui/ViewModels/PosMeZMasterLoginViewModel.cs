using System.Diagnostics;
using System.Reflection;
using CommunityToolkit.Maui.Core;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Views;
using Unity;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.SystemNames;
using static Microsoft.Maui.Controls.Application;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using Android.Runtime;



namespace v4posme_maui.ViewModels
{
	public class PosMeZMasterLoginViewModel : BaseViewModel
	{
		private readonly RestApiCoreAcount _restServiceUser = new();
		private readonly IRepositoryTbUser _repositoryTbUser;
		private string? _userName;
		private string? _password;
		private bool _opcionPagar;
		private string? _company;
		private bool _popupShow;
		private bool _remember;
		private readonly IRepositoryTbCompany _repositoryTbCompany;
		private readonly IRepositoryParameters _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();

		public PosMeZMasterLoginViewModel()
		{
			_repositoryTbUser = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbUser>();
			LoginCommand = new Command(OnLoginClicked, ValidateLogin);
			MensajeCommand = new Command(OnMensaje, ValidateError);
			PropertyChanged += (_, _) => LoginCommand.ChangeCanExecute();
			_repositoryTbCompany = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCompany>();
			RealizarPagoCommand = new Command(OnRealizarPagoCommand);
			// Obtener la versión de la aplicación
			var version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0);
			VersionApp = $"Version {version.Major}.{version.Minor}.{version.Build}";
		}
		
		public Command LoginCommand { get; }
		private Command MensajeCommand { get; }


		public bool PopupShow
		{
			get => _popupShow;
			set => SetProperty(ref _popupShow, value);
		}

		public string? UserName
		{
			get => _userName;
			set => SetProperty(ref this._userName, value);
		}

		public string? Password
		{
			get => _password;
			set => SetProperty(ref this._password, value);
		}

		public string? Company
		{
			get => _company;
			set => SetProperty(ref _company, value);
		}

		public bool Remember
		{
			get => _remember;
			set => SetProperty(ref _remember, value);
		}

		public bool OpcionPagar
		{
			get => _opcionPagar;
			set => SetProperty(ref this._opcionPagar, value);
		}

		public Command RealizarPagoCommand { get; }

		private decimal _montoSeleccionado;

		private string _versionApp=string.Empty;
		public string VersionApp
		{
			get=> _versionApp;
			set=> SetProperty(ref _versionApp, value);
		}

		public decimal MontoSeleccionado
		{
			get => _montoSeleccionado;
			set => SetProperty(ref _montoSeleccionado, value);
		}

		private async void OnRealizarPagoCommand(object obj)
		{
			if (string.IsNullOrWhiteSpace(Company))
			{
				ShowToast(Mensajes.MensajeCompania, ToastDuration.Long, 12);
			}

			if (!ValidateLogin())
			{
				return;
			}


			if (decimal.Compare(MontoSeleccionado, decimal.Zero) <= 0)
			{
				ShowToast(Mensajes.MensajeMontoMenorIgualCero, ToastDuration.Long, 12);
				return;
			}

			await Navigation!.PushModalAsync(new LoadingPage());
			VariablesGlobales.CompanyKey = Company!.ToLower();
			var findUserRemember =
				await _repositoryTbUser.PosMeFindUserByNicknameAndPassword(UserName!, Password!);
			if (Remember)
			{
				var response = await _restServiceUser.LoginMobile(UserName!, Password!);
				if (response is null)
				{
					Mensaje = Mensajes.MensajeCredencialesInvalida;
					PopupShow = true;
					await Navigation.PopModalAsync();
					return;
				}

				PopupShow = false;
			}
			else
			{
				if (await _repositoryTbUser.PosMeRowCount() <= 0)
				{
					Mensaje = Mensajes.MensajeSinDatosTabla;
					MensajeCommand.Execute(null);
					PopupShow = true;
					await Navigation.PopModalAsync();
					return;
				}

				if (findUserRemember is null)
				{
					Mensaje = Mensajes.MensajeCredencialesInvalida;
					MensajeCommand.Execute(null);
					PopupShow = true;
					await Navigation.PopModalAsync();
					return;
				}
			}

			var realizarPago = new RestApiPagadito();
			var product = new List<Api_AppMobileApi_GetDataDownloadItemsResponse>
			{
				new()
				{
					Quantity = 1,
					Name = Constantes.DescripcionRealizarPago,
					PrecioPublico = MontoSeleccionado * VariablesGlobales.TipoCambio
				}
			};
			var tm = new TbTransactionMaster
			{
				Amount = MontoSeleccionado * VariablesGlobales.TipoCambio,
				CurrencyId = TypeCurrency.Cordoba
			};

			try
			{
				var uid = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_USUARIO");
				var awk = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_CLAVE");
				var operationRequest = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_OPERTATIONID_CONNECT");
				var operationExec = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_OPERTATIONID_EXEC");
				var responseUrlPago = await realizarPago.GenerarUrl(uid!.Value!, awk!.Value!, "http://posme.net",
					operationRequest!.Value!, operationExec!.Value!, product, tm);
				if (responseUrlPago is not null)
				{
					await OpenUrl(responseUrlPago.Value);
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine(e.Message);
				Mensaje = Mensajes.MensajeCredencialesInvalida;
				PopupShow = true;
			}

			await Navigation.PopModalAsync();
		}


		private async void OnLoginClicked()
		{
			try
			{
				if (string.IsNullOrWhiteSpace(Company))
				{
					return;
				}
				
				await Navigation!.PushModalAsync(new LoadingPage());
				var counterUser				 = await _repositoryTbUser.PosMeRowCount();
				VariablesGlobales.CompanyKey = Company.ToLower();
				
				if (Remember)
				{
					VariablesGlobales.User = await _restServiceUser.LoginMobile(UserName!, Password!);
					if (VariablesGlobales.User is null)
					{
						ShowMensajePopUp(Mensajes.MensajeCredencialesInvalida);
						PopupShow = true;
						await Navigation.PopModalAsync();
						return;
					}
					VariablesGlobales.User.Company = Company;
					VariablesGlobales.User.Remember = true;
					PopupShow = false;
					if (counterUser > 0)
					{
						var findUser = await _repositoryTbUser.PosMeFindFirst();
						var validate = await _repositoryTbUser.PosMeValidateUser(VariablesGlobales.User, findUser);
						if (validate)
						{
							await Navigation.PopModalAsync();
							ShowMensajePopUp(Mensajes.UsuarioNoPermitido);
							PopupShow = true;
							return;
						}

						await _repositoryTbUser.PosMeDeleteAll();
					}

					await _repositoryTbUser.PosMeInsert(VariablesGlobales.User);
				}
				else
				{
					if (counterUser <= 0)
					{
						ShowMensajePopUp(Mensajes.MensajeSinDatosTabla);
						PopupShow = true;
						await Navigation.PopModalAsync();
						return;
					}
					var findUserRemember = await _repositoryTbUser.PosMeFindUserByNicknameAndPassword(UserName!, Password!);
					if (findUserRemember is null)
					{
						await Navigation.PopModalAsync();
						ShowMensajePopUp(Mensajes.MensajeCredencialesInvalida);
						PopupShow = true;
						return;
					}

					VariablesGlobales.User = findUserRemember;
				}


				App.StartGpsService();
				VariablesGlobales.TbCompany = await _repositoryTbCompany.PosMeFindFirst();
				Current!.MainPage			= new MainPage();
				await Navigation.PopModalAsync();
			}
			catch (Exception e)
			{
				ShowMensajePopUp(e.Message);
			}
		}

		private void OnMensaje()
		{
		}

		private bool ValidateError()
		{
			return PopupShow;
		}

		private bool ValidateLogin()
		{
			return !string.IsNullOrWhiteSpace(UserName)
				   && !string.IsNullOrWhiteSpace(Password)
				   && !string.IsNullOrWhiteSpace(Company)
				   && UserName.Length > 3
				   && Password.Length > 3
				   && Company.Length > 3;
		}

		public async void OnAppearing(INavigation navigation)
		{
			Navigation = navigation;
			var findUserRemember = await _repositoryTbUser.PosmeFindUserRemember();
			if (findUserRemember is null) return;
			UserName = findUserRemember.Nickname!;
			Password = findUserRemember.Password!;
			Company = findUserRemember.Company!;
		}
	}
}