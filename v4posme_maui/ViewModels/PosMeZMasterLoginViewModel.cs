using System.Diagnostics;
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
                if (!response)
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

        public decimal MontoSeleccionado
        {
            get => _montoSeleccionado;
            set => SetProperty(ref _montoSeleccionado, value);
        }


        private async void OnLoginClicked()
        {
            await Navigation!.PushModalAsync(new LoadingPage());
            VariablesGlobales.CompanyKey = Company!.ToLower();
            var findUserRemember =
                await _repositoryTbUser.PosMeFindUserByNicknameAndPassword(UserName!, Password!);
            if (Remember)
            {

                var response = await _restServiceUser.LoginMobile(UserName!, Password!);
                if (!response)
                {
                    Mensaje = Mensajes.MensajeCredencialesInvalida;
                    PopupShow = true;
                    await Navigation.PopModalAsync();
                    return;
                }
                else
                {
                    PopupShow = false;
                }

                await _repositoryTbUser.PosMeOnRemember();
                if (findUserRemember is not null)
                {
                    findUserRemember.Remember = true;
                    findUserRemember.Company = Company;
                    await _repositoryTbUser.PosMeUpdate(findUserRemember);
                }
                else
                {
                    VariablesGlobales.User!.Company = Company;
                    VariablesGlobales.User.Remember = true;
                    await _repositoryTbUser.PosMeInsert(VariablesGlobales.User);
                }
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

                VariablesGlobales.User = findUserRemember;
            }

            VariablesGlobales.TbCompany = await _repositoryTbCompany.PosMeFindFirst();

            v4posme_maui.App.StartLocationService();

			Current!.MainPage = new MainPage();
            await Navigation.PopModalAsync();
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