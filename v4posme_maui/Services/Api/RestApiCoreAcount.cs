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
    

    public async Task<Api_CoreAccount_LoginMobileObjUserResponse?> LoginMobile(string nickname, string password)
    {
        try
        {
            var tempUrl = Constantes.UrlRequestLogin.Replace("{UrlBase}", VariablesGlobales.CompanyKey);
            var nvc = new List<KeyValuePair<string, string>>
            {
                new("txtNickname", nickname),
                new("txtPassword", password)
            };
            var req = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = new FormUrlEncodedContent(nvc)
            };
            var response = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode) return null;
            var responseBody = await response.Content.ReadAsStringAsync();
            var apiResponse = JsonConvert.DeserializeObject<Api_CoreAcount_LoginMobileResponse>(responseBody);
            if (apiResponse is null || apiResponse.Error) return null;
            return apiResponse.ObjUser;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    
}