using Newtonsoft.Json;

namespace v4posme_maui.Models;

// Representa cada opcion del parametro APP_MOBILE_SWITCH_COMPANY, cuyo valor es un
// arreglo JSON con el nombre y la URL base de cada compania disponible para cambiar.
public class DtoSwitchCompany
{
    [JsonProperty("companyName")]
    public string? CompanyName { get; set; }

    [JsonProperty("companyUrl")]
    public string? CompanyUrl { get; set; }
}
