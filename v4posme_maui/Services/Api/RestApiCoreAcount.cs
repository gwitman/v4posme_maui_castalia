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
    private readonly HttpClient _httpClient                         = new();
    private readonly IRepositoryTbParameterSystem _parameterSystem  = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
    

    public async Task<Api_CoreAccount_LoginMobileObjUserResponse?> LoginMobile(string nickname, string password)
    {
        try
        {
            var tempUrl = Constantes.UrlRequestLogin.Replace("{UrlBase}", VariablesGlobales.CompanyKey);
            var nvc     = new List<KeyValuePair<string, string>>
            {
                new("txtNickname", nickname),
                new("txtPassword", password)
            };
            var req     = new HttpRequestMessage(HttpMethod.Post, tempUrl)
            {
                Content = new FormUrlEncodedContent(nvc)
            };
            req.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Linux; Android 10) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Mobile Safari/537.36");
            req.Headers.TryAddWithoutValidation("Accept", "application/json, text/html, */*");
            var response        = await _httpClient.SendAsync(req);
            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"HTTP {(int)response.StatusCode}: {errorBody}");
                return null;
            }

            var responseBody    = await response.Content.ReadAsStringAsync();
            var apiResponse     = JsonConvert.DeserializeObject<Api_CoreAcount_LoginMobileResponse>(responseBody);
            if (apiResponse is null || apiResponse.Error) return null;
            return apiResponse.ObjUser;
        }
        catch (Exception ex)
        {
            return null;
        }
    }

    
}