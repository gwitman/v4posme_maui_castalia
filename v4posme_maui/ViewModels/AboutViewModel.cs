using CommunityToolkit.Maui.Core;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.ViewModels
{
    public class AboutViewModel : BaseViewModel
    {
        private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster;
        private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization;
        private readonly IRepositoryServerTransactionMaster _repositoryServerTransactionMaster;

        public const string ViewName = "AboutPage";

        public AboutViewModel()
        {
            Title = "Inicio";
            _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
            _repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
            _repositoryServerTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryServerTransactionMaster>();
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

        public async void OnAppearing(INavigation navigation)
        {
            try
            {
                IsBusy                                          = true;
                Navigation                                      = navigation;
                var findAllDocumentCreditAmortization           = await _repositoryDocumentCreditAmortization.PosMeFindByMaxDate(DateTime.Now);
                var findAll                                     = await _repositoryTbTransactionMaster.PosMeFindAll();
                var findServerTransactionMasterAbonosCordoba    = await _repositoryServerTransactionMaster.PosMeFilterByCurrencyIDAndTransactionID((int)TypeCurrency.Cordoba, (int)TypeTransaction.TransactionShare);
                var listaAbonosDolares                  = new List<TbTransactionMaster>();
                var listaAbonosCordobas                 = new List<TbTransactionMaster>();
                var listaFacturasCreditoCordobas        = new List<TbTransactionMaster>();
                var listaFacturasCreditoDolares         = new List<TbTransactionMaster>();
                var listaFacturasContadoCordobas        = new List<TbTransactionMaster>();
                var listaFacturasContadoDolares         = new List<TbTransactionMaster>();
                
                

                //Obtener las tansacciones locales
                foreach (var master in findAll)
                {
                    if (master.TransactionId == TypeTransaction.TransactionShare)
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
                    else if (master.TransactionId == TypeTransaction.TransactionInvoiceBilling)
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
                MontoFacturasContadoCordobas    = listaFacturasContadoCordobas.Sum(master => master.SubAmount);
                MontoFacturasContadoDolares     = listaFacturasContadoDolares.Sum(master => master.SubAmount);
                //Facturas Credito
                CantidadFacutrasCredito         = listaFacturasCreditoCordobas.Count + listaFacturasCreditoDolares.Count;
                MontoFacturasCreditoCordobas    = listaFacturasCreditoCordobas.Sum(master => master.SubAmount);
                MontoFacturasCreditoDolares     = listaFacturasCreditoDolares.Sum(master => master.SubAmount);
                //Totales
                TotalCordobas   = MontoAbonosCordobas + MontoFacturasContadoCordobas;
                TotalDolares    = MontoAbonosDolares + MontoFacturasContadoDolares;
                IsBusy          = false;
            }
            catch (Exception e)
            {
                ShowToast(e.Message,ToastDuration.Long, 14);
            }
        }
    }
}