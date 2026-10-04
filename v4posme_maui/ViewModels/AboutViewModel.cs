using System.Collections.ObjectModel;
using CommunityToolkit.Maui.Core;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services;
using v4posme_maui.Services.Api;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Views;
using Unity;
using static Microsoft.Maui.Controls.Application;

namespace v4posme_maui.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
        private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization;
        private readonly IRepositoryServerTransactionMaster _repositoryServerTransactionMaster;
        private readonly IRepositoryParameters _repositoryParameters;
        private readonly IRepositoryTbUser _repositoryTbUser;
        private readonly HelperCore _helperContador;
        private readonly RestApiCoreAcount _restApiCoreAcount = new();

        public const string ViewName = "AboutPage";

        public AboutViewModel()
        {
            Title = "Inicio";
            _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
            _repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
            _repositoryServerTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryServerTransactionMaster>();
            _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
            _repositoryTbUser = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbUser>();
            _helperContador = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        }

        // ===== Cambio de compania (parametro APP_MOBILE_SWITCH_COMPANY) =====

        // Lista de companias disponibles extraidas del parametro (JSON con companyName/companyUrl).
        // En el combo se muestran los nombres (companyName).
        public ObservableCollection<string> CompaniasDisponibles { get; } = new();

        // Mapa nombre -> url para resolver la URL base al cambiar de compania.
        private readonly List<DtoSwitchCompany> _companias = new();

        // Controla la visibilidad del combo. Solo se muestra si el parametro existe
        // y contiene al menos una opcion valida.
        private bool _mostrarComboCompanias;

        public bool MostrarComboCompanias
        {
            get => _mostrarComboCompanias;
            set => SetProperty(ref _mostrarComboCompanias, value);
        }

        // El combo solo se puede cambiar cuando el contador de transacciones es 0.
        private bool _comboCompaniasHabilitado;

        public bool ComboCompaniasHabilitado
        {
            get => _comboCompaniasHabilitado;
            set
            {
                if (SetProperty(ref _comboCompaniasHabilitado, value))
                {
                    OnPropertyChanged(nameof(ComboCompaniasBloqueado));
                }
            }
        }

        // Inverso de ComboCompaniasHabilitado: true cuando hay transacciones pendientes
        // y por tanto no se permite cambiar de compania.
        public bool ComboCompaniasBloqueado => !_comboCompaniasHabilitado;

        // Compania actualmente seleccionada en el combo.
        private string? _companiaSeleccionada;

        public string? CompaniaSeleccionada
        {
            get => _companiaSeleccionada;
            set => SetProperty(ref _companiaSeleccionada, value);
        }

        // Evita que el cambio inicial (al cargar la lista) dispare el flujo de cambio.
        private bool _cargandoCombo;

        // Controla la visibilidad de todos los indicadores del resumen del dia.
        // Los indicadores se ocultan si y solo si el usuario tiene activado el permiso
        // app_inventory_item_index_aspx / Updated / All.
        private bool _mostrarIndicadores = true;

        public bool MostrarIndicadores
        {
            get => _mostrarIndicadores;
            set => SetProperty(ref _mostrarIndicadores, value);
        }

        private decimal _totalCorodbas;

        public decimal TotalCordobas
        {
            get => _totalCorodbas;
            set => SetProperty(ref _totalCorodbas, value);
        }

        private decimal _totalDolares;

        public decimal TotalDolares
        {
            get => _totalDolares;
            set => SetProperty(ref _totalDolares, value);
        }

        private int _cantidadAbonos;

        public int CantidadAbonos
        {
            get => _cantidadAbonos;
            set => SetProperty(ref _cantidadAbonos, value);
        }

        private decimal _montoAbonosCordobas;

        public decimal MontoAbonosCordobas
        {
            get => _montoAbonosCordobas;
            set => SetProperty(ref _montoAbonosCordobas, value);
        }

        private decimal _montoAbonosDolares;

        public decimal MontoAbonosDolares
        {
            get => _montoAbonosDolares;
            set => SetProperty(ref _montoAbonosDolares, value);
        }
        private decimal _montoTotalAbonosCordobas;

        public decimal MontoTotalAbonosCordobas
        {
            get => _montoTotalAbonosCordobas;
            set => SetProperty(ref _montoTotalAbonosCordobas, value);
        }
        
        private decimal _montoTotalAbonosDolares;

        public decimal MontoTotalAbonosDolares
        {
            get => _montoTotalAbonosDolares;
            set => SetProperty(ref _montoTotalAbonosDolares, value);
        }
        private decimal _porcentajeAbonosTotalesCordobas;

        public decimal PorcentajeAbonosTotalesCordobas
        {
            get => _porcentajeAbonosTotalesCordobas;
            set => SetProperty(ref _porcentajeAbonosTotalesCordobas, value);
        }
        
        private decimal _porcentajeAbonosTotalesDolares;

        public decimal PorcentajeAbonosTotalesDolares
        {
            get => _porcentajeAbonosTotalesDolares;
            set => SetProperty(ref _porcentajeAbonosTotalesDolares, value);
        }
        
        private int _cantidadFacutrasContado;

        public int CantidadFacutrasContado
        {
            get => _cantidadFacutrasContado;
            set => SetProperty(ref _cantidadFacutrasContado, value);
        }

        private decimal _montoFacturasContadoCordobas;

        public decimal MontoFacturasContadoCordobas
        {
            get => _montoFacturasContadoCordobas;
            set => SetProperty(ref _montoFacturasContadoCordobas, value);
        }

        private decimal _montoFacturasContadoDolares;

        public decimal MontoFacturasContadoDolares
        {
            get => _montoFacturasContadoDolares;
            set => SetProperty(ref _montoFacturasContadoDolares, value);
        }

        private int _cantidadFacutrasCredito;

        public int CantidadFacutrasCredito
        {
            get => _cantidadFacutrasCredito;
            set => SetProperty(ref _cantidadFacutrasCredito, value);
        }

        private decimal _montoFacturasCreditoCordobas;

        public decimal MontoFacturasCreditoCordobas
        {
            get => _montoFacturasCreditoCordobas;
            set => SetProperty(ref _montoFacturasCreditoCordobas, value);
        }

        private decimal _montoFacturasCreditoDolares;

        public decimal MontoFacturasCreditoDolares
        {
            get => _montoFacturasCreditoDolares;
            set => SetProperty(ref _montoFacturasCreditoDolares, value);
        }

        private int _cantidadGastos;

        public int CantidadGastos
        {
            get => _cantidadGastos;
            set => SetProperty(ref _cantidadGastos, value);
        }

        private decimal _montoGastosCordobas;

        public decimal MontoGastosCordobas
        {
            get => _montoGastosCordobas;
            set => SetProperty(ref _montoGastosCordobas, value);
        }

        private decimal _montoGastosDolares;

        public decimal MontoGastosDolares
        {
            get => _montoGastosDolares;
            set => SetProperty(ref _montoGastosDolares, value);
        }

        private int _cantidadIngresos;

        public int CantidadIngresos
        {
            get => _cantidadIngresos;
            set => SetProperty(ref _cantidadIngresos, value);
        }

        private decimal _montoIngresosCordobas;

        public decimal MontoIngresosCordobas
        {
            get => _montoIngresosCordobas;
            set => SetProperty(ref _montoIngresosCordobas, value);
        }

        private decimal _montoIngresosDolares;

        public decimal MontoIngresosDolares
        {
            get => _montoIngresosDolares;
            set => SetProperty(ref _montoIngresosDolares, value);
        }

        // Salidas de efectivo (Cash Outflow): disminuyen la caja.
        private int _cantidadEgresos;

        public int CantidadEgresos
        {
            get => _cantidadEgresos;
            set => SetProperty(ref _cantidadEgresos, value);
        }

        private decimal _montoEgresosCordobas;

        public decimal MontoEgresosCordobas
        {
            get => _montoEgresosCordobas;
            set => SetProperty(ref _montoEgresosCordobas, value);
        }

        private decimal _montoEgresosDolares;

        public decimal MontoEgresosDolares
        {
            get => _montoEgresosDolares;
            set => SetProperty(ref _montoEgresosDolares, value);
        }

        // Ingreso general = facturas de contado + abonos + ingresos de efectivo.
        private decimal _ingresoGeneralCordobas;

        public decimal IngresoGeneralCordobas
        {
            get => _ingresoGeneralCordobas;
            set => SetProperty(ref _ingresoGeneralCordobas, value);
        }

        private decimal _ingresoGeneralDolares;

        public decimal IngresoGeneralDolares
        {
            get => _ingresoGeneralDolares;
            set => SetProperty(ref _ingresoGeneralDolares, value);
        }

        // Compras / entradas de inventario (aumentan existencia).
        private int _cantidadEntradasInventario;

        public int CantidadEntradasInventario
        {
            get => _cantidadEntradasInventario;
            set => SetProperty(ref _cantidadEntradasInventario, value);
        }

        private decimal _montoEntradasInventarioCordobas;

        public decimal MontoEntradasInventarioCordobas
        {
            get => _montoEntradasInventarioCordobas;
            set => SetProperty(ref _montoEntradasInventarioCordobas, value);
        }

        private decimal _montoEntradasInventarioDolares;

        public decimal MontoEntradasInventarioDolares
        {
            get => _montoEntradasInventarioDolares;
            set => SetProperty(ref _montoEntradasInventarioDolares, value);
        }

        // Salidas de inventario (disminuyen existencia).
        private int _cantidadSalidasInventario;

        public int CantidadSalidasInventario
        {
            get => _cantidadSalidasInventario;
            set => SetProperty(ref _cantidadSalidasInventario, value);
        }

        private decimal _montoSalidasInventarioCordobas;

        public decimal MontoSalidasInventarioCordobas
        {
            get => _montoSalidasInventarioCordobas;
            set => SetProperty(ref _montoSalidasInventarioCordobas, value);
        }

        private decimal _montoSalidasInventarioDolares;

        public decimal MontoSalidasInventarioDolares
        {
            get => _montoSalidasInventarioDolares;
            set => SetProperty(ref _montoSalidasInventarioDolares, value);
        }

        // Ajuste de inventario.
        private int _cantidadAjusteInventario;

        public int CantidadAjusteInventario
        {
            get => _cantidadAjusteInventario;
            set => SetProperty(ref _cantidadAjusteInventario, value);
        }

        private decimal _montoAjusteInventarioCordobas;

        public decimal MontoAjusteInventarioCordobas
        {
            get => _montoAjusteInventarioCordobas;
            set => SetProperty(ref _montoAjusteInventarioCordobas, value);
        }

        private decimal _montoAjusteInventarioDolares;

        public decimal MontoAjusteInventarioDolares
        {
            get => _montoAjusteInventarioDolares;
            set => SetProperty(ref _montoAjusteInventarioDolares, value);
        }

        // Otras entradas de inventario (aumentan existencia).
        private int _cantidadOtraEntradaInventario;

        public int CantidadOtraEntradaInventario
        {
            get => _cantidadOtraEntradaInventario;
            set => SetProperty(ref _cantidadOtraEntradaInventario, value);
        }

        private decimal _montoOtraEntradaInventarioCordobas;

        public decimal MontoOtraEntradaInventarioCordobas
        {
            get => _montoOtraEntradaInventarioCordobas;
            set => SetProperty(ref _montoOtraEntradaInventarioCordobas, value);
        }

        private decimal _montoOtraEntradaInventarioDolares;

        public decimal MontoOtraEntradaInventarioDolares
        {
            get => _montoOtraEntradaInventarioDolares;
            set => SetProperty(ref _montoOtraEntradaInventarioDolares, value);
        }

        public async void OnAppearing(INavigation navigation)
        {
            try
            {
                IsBusy                                          = true;
                Navigation                                      = navigation;
                // Ocultar los indicadores si el usuario tiene el permiso activado.
                bool permission                                 = await _helperContador.GetPermission(TypeMenuElementID.core_dashboards, TypePermission.Selected, TypeImpact.None);
                MostrarIndicadores                              = !permission;
                await CargarComboCompanias();
                var findAllDocumentCreditAmortization           = await _repositoryDocumentCreditAmortization.PosMeFindByMaxDate(DateTime.Now);
                var findAll                                     = await _repositoryTbTransactionMaster.PosMeFindAll();
                var findServerTransactionMasterAbonosCordoba    = await _repositoryServerTransactionMaster.PosMeFilterByCurrencyIDAndTransactionID((int)TypeCurrency.Cordoba, (int)TypeTransaction.TransactionShare);
                var listaAbonosDolares                  = new List<TbTransactionMaster>();
                var listaAbonosCordobas                 = new List<TbTransactionMaster>();
                var listaFacturasCreditoCordobas        = new List<TbTransactionMaster>();
                var listaFacturasCreditoDolares         = new List<TbTransactionMaster>();
                var listaFacturasContadoCordobas        = new List<TbTransactionMaster>();
                var listaFacturasContadoDolares         = new List<TbTransactionMaster>();
                var listaGastosCordobas                 = new List<TbTransactionMaster>();
                var listaGastosDolares                  = new List<TbTransactionMaster>();
                var listaIngresosCordobas               = new List<TbTransactionMaster>();
                var listaIngresosDolares                = new List<TbTransactionMaster>();
                var listaEgresosCordobas                = new List<TbTransactionMaster>();
                var listaEgresosDolares                 = new List<TbTransactionMaster>();
                var listaEntradasInventarioCordobas     = new List<TbTransactionMaster>();
                var listaEntradasInventarioDolares      = new List<TbTransactionMaster>();
                var listaSalidasInventarioCordobas      = new List<TbTransactionMaster>();
                var listaSalidasInventarioDolares       = new List<TbTransactionMaster>();
                var listaAjusteInventarioCordobas       = new List<TbTransactionMaster>();
                var listaAjusteInventarioDolares        = new List<TbTransactionMaster>();
                var listaOtraEntradaInventarioCordobas  = new List<TbTransactionMaster>();
                var listaOtraEntradaInventarioDolares   = new List<TbTransactionMaster>();
                
                

                //Obtener las tansacciones locales
                foreach (var master in findAll)
                {
                    if (master.TransactionId == TypeTransaction.TransactionExpense)
                    {
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaGastosCordobas.Add(master);
                        }
                        else
                        {
                            listaGastosDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionCashInflow)
                    {
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaIngresosCordobas.Add(master);
                        }
                        else
                        {
                            listaIngresosDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionCashOutflow)
                    {
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaEgresosCordobas.Add(master);
                        }
                        else
                        {
                            listaEgresosDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionShare)
                    {
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaAbonosCordobas.Add(master);
                        }
                        else
                        {
                            listaAbonosDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionInventarioCompras)
                    {
                        // Compras / entradas de inventario (aumentan existencia).
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaEntradasInventarioCordobas.Add(master);
                        }
                        else
                        {
                            listaEntradasInventarioDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionInventarioSalida)
                    {
                        // Salidas de inventario (disminuyen existencia).
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaSalidasInventarioCordobas.Add(master);
                        }
                        else
                        {
                            listaSalidasInventarioDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionInventarioAjuste)
                    {
                        // Ajuste de inventario.
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaAjusteInventarioCordobas.Add(master);
                        }
                        else
                        {
                            listaAjusteInventarioDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionInventarioEntradas)
                    {
                        // Otras entradas de inventario (aumentan existencia).
                        if (master.CurrencyId == TypeCurrency.Cordoba)
                        {
                            listaOtraEntradaInventarioCordobas.Add(master);
                        }
                        else
                        {
                            listaOtraEntradaInventarioDolares.Add(master);
                        }
                    }
                    else if (master.TransactionId == TypeTransaction.TransactionInvoiceBilling  && master.StatusID == (int)TypeStatusBilling.Apply  && master.RegisterLocal == 1 )
                    {
                        if (master.TransactionCausalId == TypeTransactionCausal.Credito)
                        {
                            if (master.CurrencyId == TypeCurrency.Cordoba)
                            {
                                listaFacturasCreditoCordobas.Add(master);
                            }
                            else
                            {
                                listaFacturasCreditoDolares.Add(master);
                            }
                        }
                        else
                        {
                            if (master.CurrencyId == TypeCurrency.Cordoba)
                            {
                                listaFacturasContadoCordobas.Add(master);
                            }
                            else
                            {
                                listaFacturasContadoDolares.Add(master);
                            }
                        }
                    }
                }


                //Obtener las transacciones del server
                foreach (var master in findServerTransactionMasterAbonosCordoba)
                {
                    TbTransactionMaster t   = new TbTransactionMaster();
                    t.SubAmount             = master.Amount;
                    listaAbonosCordobas.Add(t);
                }

                //Abonos
                CantidadAbonos      = listaAbonosCordobas.Count + listaAbonosDolares.Count;
                MontoAbonosCordobas = listaAbonosCordobas.Sum(master => master.SubAmount);
                MontoAbonosDolares  = listaAbonosDolares.Sum(master => master.SubAmount);
                foreach (var documentCredit in findAllDocumentCreditAmortization)
                {
                    if (documentCredit.CurrencyId ==TypeCurrency.Cordoba)
                    {
                        MontoTotalAbonosCordobas        = findAllDocumentCreditAmortization.Sum(response => response.Balance);
                        PorcentajeAbonosTotalesCordobas = Math.Round(MontoAbonosCordobas / MontoTotalAbonosCordobas * 100, 2);
                    }
                    else
                    {
                        MontoTotalAbonosDolares         = findAllDocumentCreditAmortization.Sum(response => response.Balance);
                        PorcentajeAbonosTotalesDolares  = Math.Round(MontoAbonosDolares / MontoTotalAbonosCordobas * 100, 2);
                    }
                }
                
                //Facutras Contado
                CantidadFacutrasContado         = listaFacturasContadoCordobas.Count + listaFacturasContadoDolares.Count;
                MontoFacturasContadoCordobas    = listaFacturasContadoCordobas.Sum(master => master.SubAmount) - listaFacturasContadoCordobas.Sum(master => master.Discount);
                MontoFacturasContadoDolares     = listaFacturasContadoDolares.Sum(master => master.SubAmount) - listaFacturasContadoDolares.Sum(master => master.Discount) ;
                //Facturas Credito
                CantidadFacutrasCredito         = listaFacturasCreditoCordobas.Count + listaFacturasCreditoDolares.Count;
                MontoFacturasCreditoCordobas    = listaFacturasCreditoCordobas.Sum(master => master.SubAmount) - listaFacturasCreditoCordobas.Sum(master => master.Discount);
                MontoFacturasCreditoDolares     = listaFacturasCreditoDolares.Sum(master => master.SubAmount) - listaFacturasCreditoDolares.Sum(master => master.Discount);
                //Gastos
                CantidadGastos      = listaGastosCordobas.Count + listaGastosDolares.Count;
                MontoGastosCordobas = listaGastosCordobas.Sum(master => master.Amount);
                MontoGastosDolares  = listaGastosDolares.Sum(master => master.Amount);
                //Ingresos de efectivo
                CantidadIngresos      = listaIngresosCordobas.Count + listaIngresosDolares.Count;
                MontoIngresosCordobas = listaIngresosCordobas.Sum(master => master.Amount);
                MontoIngresosDolares  = listaIngresosDolares.Sum(master => master.Amount);
                //Salidas de efectivo (egresos)
                CantidadEgresos      = listaEgresosCordobas.Count + listaEgresosDolares.Count;
                MontoEgresosCordobas = listaEgresosCordobas.Sum(master => master.Amount);
                MontoEgresosDolares  = listaEgresosDolares.Sum(master => master.Amount);
                //Compras / entradas de inventario
                CantidadEntradasInventario      = listaEntradasInventarioCordobas.Count + listaEntradasInventarioDolares.Count;
                MontoEntradasInventarioCordobas = listaEntradasInventarioCordobas.Sum(master => master.Amount);
                MontoEntradasInventarioDolares  = listaEntradasInventarioDolares.Sum(master => master.Amount);
                //Salidas de inventario
                CantidadSalidasInventario       = listaSalidasInventarioCordobas.Count + listaSalidasInventarioDolares.Count;
                MontoSalidasInventarioCordobas  = listaSalidasInventarioCordobas.Sum(master => master.Amount);
                MontoSalidasInventarioDolares   = listaSalidasInventarioDolares.Sum(master => master.Amount);

                //Ajuste de inventario
                CantidadAjusteInventario        = listaAjusteInventarioCordobas.Count + listaAjusteInventarioDolares.Count;
                MontoAjusteInventarioCordobas   = listaAjusteInventarioCordobas.Sum(master => master.Amount);
                MontoAjusteInventarioDolares    = listaAjusteInventarioDolares.Sum(master => master.Amount);

                //Otras entradas de inventario
                CantidadOtraEntradaInventario       = listaOtraEntradaInventarioCordobas.Count + listaOtraEntradaInventarioDolares.Count;
                MontoOtraEntradaInventarioCordobas  = listaOtraEntradaInventarioCordobas.Sum(master => master.Amount);
                MontoOtraEntradaInventarioDolares   = listaOtraEntradaInventarioDolares.Sum(master => master.Amount);
                //Ingreso general = facturas de contado + abonos + ingresos de efectivo
                IngresoGeneralCordobas = MontoFacturasContadoCordobas + MontoAbonosCordobas + MontoIngresosCordobas;
                IngresoGeneralDolares  = MontoFacturasContadoDolares + MontoAbonosDolares + MontoIngresosDolares;
                //Total del dia = ventas de contado + abonos + ingresos - compras (entradas de
                //inventario) - gastos - salidas de efectivo. Las salidas de inventario no afectan el total de caja.
                TotalCordobas   = MontoFacturasContadoCordobas + MontoAbonosCordobas + MontoIngresosCordobas - MontoEntradasInventarioCordobas - MontoGastosCordobas - MontoEgresosCordobas;
                TotalDolares    = MontoFacturasContadoDolares + MontoAbonosDolares + MontoIngresosDolares - MontoEntradasInventarioDolares - MontoGastosDolares - MontoEgresosDolares;
                IsBusy          = false;
            }
            catch (Exception e)
            {
                HelperLogs.Log(e);
                ShowToast(e.Message,ToastDuration.Long, 14);
            }
        }

        // Carga la lista de companias desde el parametro APP_MOBILE_SWITCH_COMPANY.
        // Reglas:
        //  - Si el parametro no existe -> no mostrar el combo.
        //  - Si existe pero su valor es vacio o solo "|" -> no mostrar el combo.
        //  - Si tiene valores -> split por "|" y mostrar las opciones.
        //  - El combo se habilita solo cuando el contador de transacciones es 0.
        private async Task CargarComboCompanias()
        {
            try
            {
                _cargandoCombo = true;
                CompaniasDisponibles.Clear();
                _companias.Clear();
                MostrarComboCompanias = false;

                var parametro = await _repositoryParameters.PosMeFindByKey(Constantes.AppMobileSwitchCompany);
                if (parametro is null || string.IsNullOrWhiteSpace(parametro.Value) || parametro.Value.Trim() == "|")
                {
                    return;
                }

                // El valor es un arreglo JSON de { companyName, companyUrl }.
                List<DtoSwitchCompany>? opciones;
                try
                {
                    opciones = JsonConvert.DeserializeObject<List<DtoSwitchCompany>>(parametro.Value);
                }
                catch (Exception ex)
                {
                    HelperLogs.Log(ex);
                    return;
                }

                var validas = (opciones ?? new List<DtoSwitchCompany>())
                    .Where(opcion => !string.IsNullOrWhiteSpace(opcion.CompanyName)
                                     && !string.IsNullOrWhiteSpace(opcion.CompanyUrl))
                    .ToList();

                if (validas.Count == 0)
                {
                    return;
                }

                foreach (var opcion in validas)
                {
                    _companias.Add(opcion);
                    CompaniasDisponibles.Add(opcion.CompanyName!);
                }

                // Seleccionar la compania actual (por URL base) si esta en la lista.
                var companiaActual = VariablesGlobales.CompanyKey;
                var seleccion = validas.FirstOrDefault(opcion =>
                    string.Equals(opcion.CompanyUrl, companiaActual, StringComparison.OrdinalIgnoreCase));
                CompaniaSeleccionada = (seleccion ?? validas.First()).CompanyName;

                // Habilitar el combo solo si no hay transacciones pendientes.
                var contador = await _helperContador.GetCounter();
                ComboCompaniasHabilitado = contador == 0;

                MostrarComboCompanias = true;
            }
            catch (Exception e)
            {
                HelperLogs.Log(e);
                MostrarComboCompanias = false;
            }
            finally
            {
                _cargandoCombo = false;
            }
        }

        // Invocado desde la vista cuando el usuario cambia la seleccion del combo.
        // Pide credenciales, hace login contra la nueva compania, descarga y guarda la
        // informacion. Solo si todo sale bien se actualiza el estado y el encabezado.
        public async Task OnCompaniaSeleccionadaCambio(string nuevaCompania)
        {
            if (_cargandoCombo || string.IsNullOrWhiteSpace(nuevaCompania))
            {
                return;
            }

            // Resolver la compania (nombre + url) a partir del nombre seleccionado.
            var compania = _companias.FirstOrDefault(c =>
                string.Equals(c.CompanyName, nuevaCompania, StringComparison.OrdinalIgnoreCase));
            if (compania is null || string.IsNullOrWhiteSpace(compania.CompanyUrl))
            {
                return;
            }

            // Si la seleccion es la compania actual, no hacer nada.
            if (string.Equals(compania.CompanyUrl, VariablesGlobales.CompanyKey, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Validar nuevamente el contador antes de permitir el cambio.
            var contador = await _helperContador.GetCounter();
            if (contador != 0)
            {
                ComboCompaniasHabilitado = false;
                ShowToast(Mensajes.MensajeDownloadCantidadTransacciones, ToastDuration.Long, 14);
                await RestaurarSeleccionActual();
                return;
            }

            if (Navigation is null)
            {
                return;
            }

            // Pedir usuario y contrasena mediante una pagina modal dedicada.
            var loginPage = new SwitchCompanyLoginPage(compania.CompanyName!);
            await Navigation.PushModalAsync(loginPage);
            var credenciales = await loginPage.WaitForResultAsync();

            if (!credenciales.Aceptado
                || string.IsNullOrWhiteSpace(credenciales.Usuario)
                || string.IsNullOrWhiteSpace(credenciales.Clave))
            {
                await RestaurarSeleccionActual();
                return;
            }

            await CambiarCompania(compania, credenciales.Usuario, credenciales.Clave);
        }

        private async Task CambiarCompania(DtoSwitchCompany compania, string usuario, string clave)
        {
            // Conservar el estado anterior para poder revertir si la descarga falla.
            var companyKeyAnterior  = VariablesGlobales.CompanyKey;
            var usuarioAnterior     = VariablesGlobales.User;

            try
            {
                await Navigation!.PushModalAsync(new LoadingPage());

                // La URL base de la compania se usa tal cual para construir las peticiones.
                VariablesGlobales.CompanyKey = compania.CompanyUrl;

                var usuarioServidor = await _restApiCoreAcount.LoginMobile(usuario, clave);
                if (usuarioServidor is null)
                {
                    await Navigation.PopModalAsync();
                    ShowToast(Mensajes.MensajeCredencialesInvalida, ToastDuration.Long, 14);
                    RevertirEstado(companyKeyAnterior, usuarioAnterior);
                    await RestaurarSeleccionActual();
                    return;
                }

                usuarioServidor.Company     = compania.CompanyUrl;
                usuarioServidor.Remember    = true;
                VariablesGlobales.User      = usuarioServidor;

                // Descargar y guardar la informacion de la nueva compania.
                var restApiAppMobile = new RestApiAppMobileApi();
                var resultado = await restApiAppMobile.GetDataDownload(false);
                if (resultado.Error)
                {
                    // Si falla la descarga NO se actualiza: revertir estado.
                    await Navigation.PopModalAsync();
                    ShowToast(resultado.Description, ToastDuration.Long, 14);
                    RevertirEstado(companyKeyAnterior, usuarioAnterior);
                    await RestaurarSeleccionActual();
                    return;
                }

                // Descarga correcta: persistir el usuario (recordado) para que el login
                // quede con la ultima compania seleccionada.
                await _repositoryTbUser.PosMeOnRemember();
                var usuarioLocal = await _repositoryTbUser.PosMeFindUserByNicknameAndPassword(
                    usuarioServidor.Nickname!, usuarioServidor.Password!);
                if (usuarioLocal is null)
                {
                    usuarioServidor.Remember = true;
                    await _repositoryTbUser.PosMeInsert(usuarioServidor);
                }
                else
                {
                    usuarioLocal.Remember = true;
                    usuarioLocal.Company = compania.CompanyUrl;
                    await _repositoryTbUser.PosMeUpdate(usuarioLocal);
                }

                VariablesGlobales.TbCompany = await VariablesGlobales.UnityContainer
                    .Resolve<IRepositoryTbCompany>().PosMeFindFirst();

                await Navigation.PopModalAsync();
                ShowToast(Mensajes.MensajeDownloadSuccess, ToastDuration.Long, 14);

                // Actualizar el encabezado del menu lateral y recargar indicadores.
                if (Current!.MainPage is MainPage mainPage)
                {
                    mainPage.LoadHeaderInfo();
                }

                OnAppearing(Navigation);
            }
            catch (Exception e)
            {
                HelperLogs.Log(e);
                try { await Navigation!.PopModalAsync(); } catch { /* ignore */ }
                ShowToast(Mensajes.MensajeDownloadError, ToastDuration.Long, 14);
                RevertirEstado(companyKeyAnterior, usuarioAnterior);
                await RestaurarSeleccionActual();
            }
        }

        private static void RevertirEstado(string? companyKeyAnterior, Api_CoreAccount_LoginMobileObjUserResponse? usuarioAnterior)
        {
            VariablesGlobales.CompanyKey = companyKeyAnterior;
            VariablesGlobales.User = usuarioAnterior;
        }

        // Restaura visualmente la seleccion del combo a la compania actual sin disparar
        // nuevamente el flujo de cambio.
        private async Task RestaurarSeleccionActual()
        {
            _cargandoCombo = true;
            var companiaActual = VariablesGlobales.CompanyKey;
            var seleccion = _companias.FirstOrDefault(c =>
                string.Equals(c.CompanyUrl, companiaActual, StringComparison.OrdinalIgnoreCase));
            CompaniaSeleccionada = seleccion?.CompanyName ?? CompaniasDisponibles.FirstOrDefault();
            _cargandoCombo = false;
            await Task.CompletedTask;
        }
    }
}