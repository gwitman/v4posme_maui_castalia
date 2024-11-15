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
    private readonly IRepositoryTbCustomer _repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();

    private readonly IRepositoryItems _repositoryItems = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();

    private readonly IRepositoryParameters _repositoryParameters = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();

    private readonly IRepositoryDocumentCreditAmortization _repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();

    private readonly IRepositoryDocumentCredit _repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();

    private readonly IRepositoryTbCompany _repositoryTbCompany = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCompany>();

    private readonly IRepositoryTbTransactionMaster _repositoryTbTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();
    
    private readonly IRepositoryTbTransactionMasterDetail _repositoryTbTransactionMasterDetail = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMasterDetail>();

    private readonly IRepositoryTbParameterSystem _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
    
    public async Task<bool> GetDataDownload()
    {
        var tempUrl = Constantes.UrlRequestDownload.Replace("{UrlBase}", VariablesGlobales.CompanyKey);
        if (VariablesGlobales.User is null)
        {
            return false;
        }

        try
        {
            var nickname = VariablesGlobales.User.Nickname!;
            var password = VariablesGlobales.User.Password!;
            var nvc = new List<KeyValuePair<string, string>>
            {
                new("txtNickname", nickname),
                new("txtPassword", password)
            };
            var req = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = new FormUrlEncodedContent(nvc)
            };

            //validar repuesta
            var response = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode) return false;

            var responseBody = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<Api_AppMobileApi_GetDataDownloadResponse>(responseBody);
            if (apiResponse is null || apiResponse.Error) return false;

            //eliminar movimientos
            var customerDeleteAll = _repositoryTbCustomer!.PosMeDeleteAll();
            var itemsDeleteAll = _repositoryItems!.PosMeDeleteAll();
            var documentCreditAmortizationDeleteAll = _repositoryDocumentCreditAmortization!.PosMeDeleteAll();
            var parametersDeleteAll = _repositoryParameters!.PosMeDeleteAll();
            var documentCreditDeleteAll = _repositoryDocumentCredit!.PosMeDeleteAll();
            var companyDeleteAll = _repositoryTbCompany.PosMeDeleteAll();
            await Task.WhenAll([
                customerDeleteAll, itemsDeleteAll, documentCreditAmortizationDeleteAll, parametersDeleteAll,
                documentCreditDeleteAll, companyDeleteAll
            ]);

            //insertar nuevos movimientos
            var taskCompany = _repositoryTbCompany.PosMeInsert(apiResponse.ObjCompany);
            var taskCustomer = _repositoryTbCustomer!.PosMeInsertAll(apiResponse.ListCustomer);
            var taskItem = _repositoryItems!.PosMeInsertAll(apiResponse.ListItem);
            var taskDocumentCreditAmortization = _repositoryDocumentCreditAmortization!.PosMeInsertAll(apiResponse.ListDocumentCreditAmortization);
            var taskParameters = _repositoryParameters!.PosMeInsertAll(apiResponse.ListParameter);
            var taskDocumentCredit = _repositoryDocumentCredit!.PosMeInsertAll(apiResponse.ListDocumentCredit);
            await Task.WhenAll([
                taskCustomer, taskItem, taskDocumentCreditAmortization, taskParameters,
                taskDocumentCredit, taskCompany
            ]);

            //inicializar contador 
            var objParameterSystem = await _parameterSystem.PosMeFindByName(Constantes.ParemeterEntityIDAutoIncrement);
            objParameterSystem.Value = $"-1";
            await _parameterSystem.PosMeUpdate(objParameterSystem);


            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ERROR {ex.Message}");
            return false;
        }
    }

    public async Task<string> SendDataAsync()
    {
        try
        {
            var nickname = VariablesGlobales.User!.Nickname!;
            var password = VariablesGlobales.User.Password!;
            var helper = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
            var findCustomers = await _repositoryTbCustomer.PosMeTakeModificados();
            var findItems = await _repositoryItems.PosMeTakeModificado();
            var findTransactionMaster = await _repositoryTbTransactionMaster.PosMeFindAll();
            var findTransactionMasterDetail = await _repositoryTbTransactionMasterDetail.PosMeFindAll();
            var data = new Dictionary<string, object>
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
            var req = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = content
            };
            var response = await _httpClient.SendAsync(req);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return responseBody;
            }

            return "{'status': 'false'; 'message': 'error'}";
        }
        catch (HttpRequestException ex)
        {
            return $"Error: {ex.Message}";
        }
    }
}