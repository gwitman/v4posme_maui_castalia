using System.Diagnostics;
using System.Text;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;
using Android.Content;
using static Java.Util.Jar.Attributes;
using Android.Accounts;
using System.ComponentModel;

namespace v4posme_maui.Services.Api;

public class RestApiAppMobileApi
{
    private readonly HttpClient _httpClient = new();
    private readonly IRepositoryTbCustomer _repositoryTbCustomer                                    = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
    private readonly IRepositoryTbMenuElement _repositoryTbMenuElement                              = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbMenuElement>();
    private readonly IRepositoryItems _repositoryItems                                              = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
    private readonly IRepositoryParameters _repositoryParameters                                    = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
    private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization    = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit                            = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
    private readonly IRepositoryTbCompany _repositoryTbCompany                                      = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCompany>();
    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster                  = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();    
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail      = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();
    private readonly IRepositoryServerTransactionMaster _repositoryServerTransactionMasterDetail    = VariablesGlobales.UnityContainer.Resolve<IRepositoryServerTransactionMaster>();
    private readonly IRepositoryTbParameterSystem _parameterSystem                                  = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
    private readonly IRepositoryTbCatalogItem _repositoryCatalogItem                                = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCatalogItem>();
    
    public async Task<DtoMenssage> GetDataDownload(bool onlyQuantityNew)
    {
        DtoMenssage mensaje = new DtoMenssage();
        var tempUrl         = Constantes.UrlRequestDownload.Replace("{UrlBase}", VariablesGlobales.CompanyKey);
        if (VariablesGlobales.User is null)
        {
            mensaje.Error       = true;
            mensaje.Description = Mensajes.MensajeDownloadError;
            return mensaje;
        }

        try
        {
            var nickname    = VariablesGlobales.User.Nickname!;
            var password    = VariablesGlobales.User.Password!;
            var nvc         = new List<KeyValuePair<string, string>>
            {
                new("txtNickname", nickname),
                new("txtPassword", password)
            };
            var req     = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = new FormUrlEncodedContent(nvc)
            };

            //validar repuesta
            var response    = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode) {
                mensaje.Error       = true;
                mensaje.Description = Mensajes.MensajeDownloadError;
                return mensaje;
            };

            var responseBody    = await response.Content.ReadAsStringAsync();
            var apiResponse     = JsonConvert.DeserializeObject<Api_AppMobileApi_GetDataDownloadResponse>(responseBody);
            if (apiResponse is null || apiResponse.Error) {
                mensaje.Error       = true;
                mensaje.Description = Mensajes.MensajeDownloadError;
                return mensaje;
            };

            //eliminar todo e ingresar nuevamente
            if (onlyQuantityNew == true)
            {
                bool changeInItems                                                  = false;
                List<Api_AppMobileApi_GetDataDownloadItemsResponse> objListItemNew  = new List<Api_AppMobileApi_GetDataDownloadItemsResponse>();
                foreach (var item_ in apiResponse.ListItem)
                {
                    if (item_.BarCode == "51")
                    {
                        string debug = "debug point";
                    }

                    var existsItems = await _repositoryItems.PosMeFindByItemId(item_.ItemId);
                    if (existsItems == null)
                    {
                        objListItemNew.Add(item_);
                        changeInItems = true;
                    }
                    else if(item_.Quantity > existsItems.Quantity)
                    {
                        decimal dif             = item_.Quantity - existsItems.Quantity;
                        existsItems.Quantity    = existsItems.Quantity + dif;
                        changeInItems           = true;
                        await _repositoryItems.PosMeUpdate(existsItems);

                    }

                }

                //meter todos los nuevos
                if (objListItemNew.Count > 0)
                {
                    var taskItem = _repositoryItems!.PosMeInsertAll(objListItemNew);
                    await Task.WhenAll([taskItem]);
                }



                if (changeInItems == false)
                {
                    mensaje.Error       = true;
                    mensaje.Description = Mensajes.MensajeDownloadCantidadTransacciones;
                    return mensaje;
                }
                else
                {
                    mensaje.Error       = false;
                    mensaje.Description = Mensajes.MensajeDownloadSuccessOnlyQuuantity;
                    return mensaje;
                }

            }
            else
            {
                var customerDeleteAll                   = _repositoryTbCustomer!.PosMeDeleteAll();
                var itemsDeleteAll                      = _repositoryItems!.PosMeDeleteAll();
                var documentCreditAmortizationDeleteAll = _repositoryDocumentCreditAmortization!.PosMeDeleteAll();
                var parametersDeleteAll                 = _repositoryParameters!.PosMeDeleteAll();
                var documentCreditDeleteAll             = _repositoryDocumentCredit!.PosMeDeleteAll();
                var companyDeleteAll                    = _repositoryTbCompany.PosMeDeleteAll();
                var transactionMasterAll                = _repositoryTbTransactionMaster.PosMeDeleteAll();
                var transactionMasterDetailAll          = _repositoryTbTransactionMasterDetail.PosMeDeleteAll();
                var serverTransactionMasterAll          = _repositoryServerTransactionMasterDetail.PosMeDeleteAll();
                var menuElementDeleteAll                = _repositoryTbMenuElement.PosMeDeleteAll();
                var catalogItemAll                      = _repositoryCatalogItem.PosMeDeleteAll();
                
                await Task.WhenAll([
                    customerDeleteAll, 
                    itemsDeleteAll, 
                    documentCreditAmortizationDeleteAll, 
                    parametersDeleteAll,
                    documentCreditDeleteAll, 
                    companyDeleteAll, 
                    transactionMasterAll,
                    transactionMasterDetailAll,
                    serverTransactionMasterAll,
                    menuElementDeleteAll,
                    catalogItemAll
                ]);


                //Ingresar las transacionces registradas del servidor                
                List<TbTransactionMasterDetail> objListTransactionMasterDetailNew   = new List<TbTransactionMasterDetail>();
                if (apiResponse.ListTransactionMasterRegister != null)
                {
                    if (apiResponse.ListTransactionMasterRegister.Count > 0)
                    {
                        var objTransactionMasterRegister = apiResponse.ListTransactionMasterRegister.AsEnumerable().DistinctBy(lu => lu.tm_transactionMasterID).ToList();
                        
                        foreach (var objI in objTransactionMasterRegister)
                        {
                            var objListTransactionMasterFirst               = apiResponse.ListTransactionMasterRegister.Where(k => k.tm_transactionMasterID == objI.tm_transactionMasterID).First();
                            var objListTransactionMasterAll                 = apiResponse.ListTransactionMasterRegister.Where(k => k.tm_transactionMasterID == objI.tm_transactionMasterID);
                            TbTransactionMaster objTransactionMasterTemp    = new TbTransactionMaster();
                            objTransactionMasterTemp.TransactionId          = (TypeTransaction)objI.tm_transactionID;
                            objTransactionMasterTemp.TransactionMasterId    = Convert.ToInt32(objI.tm_transactionMasterMobileID);
                            objTransactionMasterTemp.Amount                 = objI.tm_Amount;
                            objTransactionMasterTemp.TransactionOn          = objI.tm_transactionOn;
                            objTransactionMasterTemp.TransactionCausalId    = (TypeTransactionCausal)objI.tm_transactionCausalID;
                            objTransactionMasterTemp.TypePaymentId          = TypePayment.Registrar;
                            objTransactionMasterTemp.Comment                = objI.tm_note;
                            objTransactionMasterTemp.Discount               = decimal.Zero;
                            objTransactionMasterTemp.Taxi1                  = decimal.Zero;
                            objTransactionMasterTemp.ExchangeRate           = decimal.Zero; //definir
                            objTransactionMasterTemp.EntityId               = objI.tm_entityID;
                            objTransactionMasterTemp.EntitySecondaryId      = VariablesGlobales.User!.UserId.ToString();
                            objTransactionMasterTemp.TransactionNumber      = objI.tm_transactionMasterMobileNumber;
                            objTransactionMasterTemp.CurrencyId             = (TypeCurrency)objI.tm_currencyID;
                            objTransactionMasterTemp.CustomerCreditLineId   = objI.tm_customerCreditLineID;
                            objTransactionMasterTemp.CustomerIdentification = objI.tm_referenceClientIdentifier!;
                            objTransactionMasterTemp.Plazo                  = objI.tm_plazo;
                            objTransactionMasterTemp.NextVisit              = objI.tm_nextVisit ?? DateTime.MinValue;
                            objTransactionMasterTemp.FixedExpenses          = objI.tm_fixedExpenses;
                            objTransactionMasterTemp.PeriodPay              = (TypePeriodPay)objI.tm_periodPay;
                            objTransactionMasterTemp.StatusID               = objI.tm_statusID;
                            objTransactionMasterTemp.MesaID                 = objI.tm_mesaID;
                            objTransactionMasterTemp.ReferenceClientName    = objI.tm_referenceClientName!;    
                            objTransactionMasterTemp.SubAmount              = objI.tm_Amount;
                            objTransactionMasterTemp.MesaName               = objI.tm_mesaName!;
                            await _repositoryTbTransactionMaster.PosMeInsert(objTransactionMasterTemp);
                            var transactionMasterId                         = objTransactionMasterTemp.TransactionMasterId;
                            foreach (var objII in objListTransactionMasterAll)
                            {
                                TbTransactionMasterDetail objTransactionMasterDetailTemp    = new TbTransactionMasterDetail();
                                objTransactionMasterDetailTemp.Quantity                     = objII.tmd_quantity;
                                objTransactionMasterDetailTemp.UnitaryCost                  = objII.tmd_unitaryCost;
                                objTransactionMasterDetailTemp.UnitaryPrice                 = objII.tmd_unitaryPrice;
                                objTransactionMasterDetailTemp.TransactionMasterId          = transactionMasterId;
                                objTransactionMasterDetailTemp.SubAmount                    = objII.tmd_Amount;
                                objTransactionMasterDetailTemp.Discount                     = decimal.Zero;
                                objTransactionMasterDetailTemp.Tax1                         = decimal.Zero;
                                objTransactionMasterDetailTemp.Componentid                  = objII.tmd_componentID;
                                objTransactionMasterDetailTemp.ComponentItemId              = objII.tmd_componentItemID;
                                objTransactionMasterDetailTemp.ItemBarCode                  = objII.tmd_barCode!;
                                objListTransactionMasterDetailNew.Add(objTransactionMasterDetailTemp);
                            }
                            
                        }
                    }
                }

                //insertar nuevos movimientos
                var taskCompany                     = _repositoryTbCompany.PosMeInsert(apiResponse.ObjCompany);
                var taskCustomer                    = _repositoryTbCustomer.PosMeInsertAll(apiResponse.ListCustomer);
                var taskItem                        = _repositoryItems!.PosMeInsertAll(apiResponse.ListItem);
                var taskDocumentCreditAmortization  = _repositoryDocumentCreditAmortization!.PosMeInsertAll(apiResponse.ListDocumentCreditAmortization);
                var taskParameters                  = _repositoryParameters!.PosMeInsertAll(apiResponse.ListParameter);
                var taskDocumentCredit              = _repositoryDocumentCredit!.PosMeInsertAll(apiResponse.ListDocumentCredit);
                var taskServerTransactionMaster     = _repositoryServerTransactionMasterDetail!.PosMeInsertAll(apiResponse.ListServerTransactionMaster);
                var taskMenuElement                 = _repositoryTbMenuElement!.PosMeInsertAll(apiResponse.ListMenuElement);
                var taskCatalogItem                 = _repositoryCatalogItem!.PosMeInsertAll(apiResponse.ListCatalogItem);                
                var taskTransactionMasterDetail     = _repositoryTbTransactionMasterDetail.PosMeInsertAll(objListTransactionMasterDetailNew);

                await Task.WhenAll([
                    taskCustomer, 
                    taskItem, 
                    taskDocumentCreditAmortization, 
                    taskParameters,
                    taskDocumentCredit, 
                    taskCompany, 
                    taskServerTransactionMaster,
                    taskMenuElement,
                    taskCatalogItem,
                    taskTransactionMasterDetail
                ]);



                //inicializar contador 
                var objParameterSystem      = await _parameterSystem.PosMeFindByName(Constantes.ParemeterEntityIDAutoIncrement);
                objParameterSystem.Value    = $"-1";
                await _parameterSystem.PosMeUpdate(objParameterSystem);


                //actualizar compania
                VariablesGlobales.TbCompany         = await _repositoryTbCompany.PosMeFindFirst();
                VariablesGlobales.OrdenarAbonos     = true;
                VariablesGlobales.OrdenarClientes   = true;


                mensaje.Error       = false;
                mensaje.Description = Mensajes.MensajeDownloadSuccess;
                return mensaje;

            }


        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR {ex.Message}");

            mensaje.Error       = true;
            mensaje.Description = Mensajes.MensajeDownloadError;
            return mensaje;

        }
    }

    public async Task<string> SendDataAsync()
    {
        try
        {
            var nickname                    = VariablesGlobales.User!.Nickname!;
            var password                    = VariablesGlobales.User.Password!;
            var helper                      = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
            var findCustomers               = await _repositoryTbCustomer.PosMeTakeModificados();
            var findItems                   = await _repositoryItems.PosMeTakeModificado();
            var findTransactionMaster       = await _repositoryTbTransactionMaster.PosMeFindAll();
            var findTransactionMasterDetail = await _repositoryTbTransactionMasterDetail.PosMeFindAll();
            var data                        = new Dictionary<string, object>
            {
                { "ObjCustomers", findCustomers },
                { "ObjItems", findItems },
                { "ObjTransactionMaster", findTransactionMaster },
                { "ObjTransactionMasterDetail", findTransactionMasterDetail }
            };
            var jsonData = JsonConvert.SerializeObject(data);
            var nvc = new List<KeyValuePair<string, string>>
            {
                new("txtNickname", nickname),
                new("txtPassword", password),
                new("txtData", jsonData)
            };
            var content = new FormUrlEncodedContent(nvc);

            var tempUrl = Constantes.UrlUpload.Replace("{UrlBase}", VariablesGlobales.CompanyKey);            
            var req     = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = content
            };
            var response        = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode) return "{'status': 'false'; 'message': 'error'}";
            var responseBody    = await response.Content.ReadAsStringAsync();
            return responseBody;
        }
        catch (Exception ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}