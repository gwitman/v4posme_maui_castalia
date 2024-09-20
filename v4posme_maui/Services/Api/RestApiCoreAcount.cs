using System.Diagnostics;
using System.Net.Http.Json;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.Repository;
using Unity;

namespace v4posme_maui.Services.Api;

public class RestApiCoreAcount
{
    private readonly HttpClient _httpClient = new();
    private readonly IRepositoryTbParameterSystem _parameterSystem = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
    

    public async Task<bool> LoginMobile(string nickname, string password)
    {
        var tempUrl = Constantes.UrlRequestLogin.Replace("{UrlBase}", VariablesGlobales.CompanyKey);
        HelperCore _helperCore; _helperCore = VariablesGlobales.UnityContainer.Resolve<HelperCore>();
        
        try
        {
            
            var nvc = new List<KeyValuePair<string, string>>();
            nvc.Add(new KeyValuePair<string, string>("txtNickname", nickname));
            nvc.Add(new KeyValuePair<string, string>("txtPassword", password));
            var req = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = new FormUrlEncodedContent(nvc)
            };

          
            var response = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode) return false;
            var responseBody = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<Api_CoreAcount_LoginMobileResponse>(responseBody);
            if (apiResponse is null || apiResponse.Error) return false;
            VariablesGlobales.User = apiResponse.ObjUser;
            return true;
        }
        catch (Exception ex)
        {
            return false;
        }
    }

    
}