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
                    menuElementDeleteAll
                ]);

                //insertar nuevos movimientos
                var taskCompany                     = _repositoryTbCompany.PosMeInsert(apiResponse.ObjCompany);
                var taskCustomer                    = _repositoryTbCustomer.PosMeInsertAll(apiResponse.ListCustomer);
                var taskItem                        = _repositoryItems!.PosMeInsertAll(apiResponse.ListItem);
                var taskDocumentCreditAmortization  = _repositoryDocumentCreditAmortization!.PosMeInsertAll(apiResponse.ListDocumentCreditAmortization);
                var taskParameters                  = _repositoryParameters!.PosMeInsertAll(apiResponse.ListParameter);
                var taskDocumentCredit              = _repositoryDocumentCredit!.PosMeInsertAll(apiResponse.ListDocumentCredit);
                var taskServerTransactionMaster     = _repositoryServerTransactionMasterDetail!.PosMeInsertAll(apiResponse.ListServerTransactionMaster);
                var taskMenuElement                 = _repositoryTbMenuElement!.PosMeInsertAll(apiResponse.ListMenuElement);

                await Task.WhenAll([
                    taskCustomer, 
                    taskItem, 
                    taskDocumentCreditAmortization, 
                    taskParameters,
                    taskDocumentCredit, 
                    taskCompany, 
                    taskServerTransactionMaster,
                    taskMenuElement
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