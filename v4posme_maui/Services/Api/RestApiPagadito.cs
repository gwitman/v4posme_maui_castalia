using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;
using Unity;

namespace v4posme_maui.Services.Api
{
    public class RestApiPagadito
    {
        private readonly IRepositoryParameters _repositoryParameters    = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        private readonly IRepositoryTbCompany _repositoryCompany        = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCompany>();
        public string Mensaje { get; private set; } = string.Empty;

        public async Task<Api_Pagadito_Response_Exec?> GenerarUrl(string uidcommece, string awkcomerce, string urlcommerce,
            string operationRequest, string operationExec, List<Api_AppMobileApi_GetDataDownloadItemsResponse> itemsResponses,
            TbTransactionMaster transactionMaster)
        {
            try
            {
                var client = new HttpClient();

                //generar token
                var nvc = new List<KeyValuePair<string, string>>
                {
                    new("uid", uidcommece),
                    new("wsk", awkcomerce),
                    new("format_return", "json"),
                    new("operation", operationRequest)
                };
                var urlPagadito = await _repositoryParameters.PosMeFindByKey("CORE_PAYMENT_PRODUCCION_URL_PAGADITO");
                var req         = new HttpRequestMessage(HttpMethod.Post, urlPagadito!.Value)
                {
                    Content = new FormUrlEncodedContent(nvc)
                };

                var responseTokenMessage    = await client.SendAsync(req);
                if (!responseTokenMessage.IsSuccessStatusCode) return null;
                var responseBodyToken       = await responseTokenMessage.Content.ReadAsStringAsync();
                var authToken               = JsonConvert.DeserializeObject<Api_Pagadito_Response_TokenPagadito>(responseBodyToken);
                if (authToken is null)
                {
                    Mensaje = Mensajes.AuthTokenError;
                    return null;
                }

                var listaDetails = new List<Detalle>();
                foreach (var item in itemsResponses)
                {
                    var precio = decimal.Zero;
                    if (transactionMaster.CurrencyId == TypeCurrency.Dolar)
                    {
                        precio = item.PrecioPublico * VariablesGlobales.TipoCambio;
                    }
                    else
                    {
                        precio = item.PrecioPublico;
                    }

                    var detail = new Detalle
                    {
                        Quantity    = item.Quantity,
                        Description = item.Name,
                        Price       = precio,
                        UrlProduct  = urlcommerce
                    };
                    listaDetails.Add(detail);
                }

                var customParam = new Dictionary<string, string>
                {
                    { "param1", "value1" }
                };

                var simboloMoneda = transactionMaster.CurrencyId switch
                {
                    TypeCurrency.Cordoba => Mensajes.MonedaCordoba,
                    TypeCurrency.Dolar => Mensajes.MonedaDolar,
                    _ => ""
                };

                var objCompany      = await _repositoryCompany.PosMeFindFirst();
                var ern             = $"FCCLI__{objCompany.FlavorId.ToString()}__{DateTime.Now:yyyyMMddHHmmss}";
                var detallesJson    = JsonConvert.SerializeObject(listaDetails);
                var parametrosJson  = JsonConvert.SerializeObject(customParam);
                var amount          = decimal.Zero;
                if (transactionMaster.CurrencyId == TypeCurrency.Dolar)
                {
                    amount = transactionMaster.Amount * VariablesGlobales.TipoCambio;
                }
                else
                {
                    amount = transactionMaster.Amount;
                }

                var data = new List<KeyValuePair<string, string>>
                {
                    new("operation", operationExec),
                    new("token", authToken.Value),
                    new("format_return", "json"),
                    new("ern", ern),
                    new("amount", amount.ToString("N2")),
                    new("currency", simboloMoneda),
                    new("details", detallesJson),
                    new("custom_params", parametrosJson)
                };
                var request = new HttpRequestMessage
                {
                    Method      = HttpMethod.Post,
                    RequestUri  = new Uri(urlPagadito.Value!),
                    Content     = new FormUrlEncodedContent(data)
                };
                var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode();

                Mensaje = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Api_Pagadito_Response_Exec>(Mensaje);
            }
            catch (Exception e)
            {
                Mensaje = e.Message;
                return null;
            }
        }
    }
}