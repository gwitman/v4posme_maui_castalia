using System.Diagnostics;
using AndroidX.Annotations;
using DevExpress.Utils.Filtering;
using Newtonsoft.Json;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Helpers;

public class HelperCore(
    IRepositoryTbParameterSystem repositoryParameters,
    IRepositoryParameters _repositoryParametersWeb,
    IRepositoryTbCustomer _repositoryTbCustomer,
    IRepositoryTbMenuElement _reporitoryTbMenuElement 
)
{

    public async Task<bool> GetPermission(TypeMenuElementID menuElementID, TypePermission typePermission,TypeImpact impact )
    {
        var findMenuElement = await _reporitoryTbMenuElement.PosMeFindById((int)menuElementID);
        if(findMenuElement is null )
            return false;

        if(typePermission == TypePermission.Updated)
        {
            if (findMenuElement.Edited == (int)impact)
                return true;
        }
        return false;
    } 
    public string ExtractCompanyKey(string companyUrl)
    {
        if (Uri.TryCreate(companyUrl, UriKind.Absolute, out var uri))
        {
            var segments = uri.Segments;
            if (segments.Length >= 3)
            {
                // El tercer segmento es la compañía (en el ejemplo: /demo/)
                return segments[2].Trim('/');
            }
        }

        // Si no es URL, puede ser que ya sea simplemente el nombre de la compañía
        return companyUrl.Trim('/');
    }

    public async Task<int> GetCounter()
    {
        var find = await repositoryParameters.PosMeFindCounter();
        return Convert.ToInt32(find.Value);
    }


    public async Task PlusCounter()
    {
        var find = await repositoryParameters.PosMeFindCounter();
        var value = Convert.ToInt32(find.Value) + 1;
        find.Value = $"{value}";
        await repositoryParameters.PosMeUpdate(find);
    }

    public async Task<string> GetCodigoAbono()
    {
        var find    = await repositoryParameters.PosMeFindCodigoAbono();
        var codigo  = find.Value!;

        if (codigo.IndexOf("-", StringComparison.Ordinal) < 0)
            throw new System.Exception(Mensajes.MnesajeCountadoDeAbonoMalFormado);

        var prefix  = find.Value!.Split("-")[0];
        var counter = find.Value!.Split("-")[1];
        var numero  = Convert.ToInt32(counter);
        numero      += 1;
        var nuevoCodigoAbono = prefix + "-" + Convert.ToString(numero).PadLeft(4, '0');
        find.Value  = nuevoCodigoAbono;
        await repositoryParameters.PosMeUpdate(find);

        return codigo;
    }

    public async Task<string> GetCodigoVisita()
    {
        var find    = await repositoryParameters.PosMeFindCodigoVisita();
        var codigo  = find.Value!;

        if (codigo.IndexOf("-", StringComparison.Ordinal) < 0)
        {
            throw new System.Exception(Mensajes.MensajeCountadorDeVisitaMalFormado);
        }

        var prefix  = find.Value!.Split("-")[0];
        var counter = find.Value!.Split("-")[1];
        var numero  = Convert.ToInt32(counter);
        numero      += 1;
        var nuevoCodigoAbono = prefix + "-" + Convert.ToString(numero).PadLeft(4, '0');
        find.Value  = nuevoCodigoAbono;
        await repositoryParameters.PosMeUpdate(find);

        return codigo;
    }

    public async Task<int> GetAutoIncrement()
    {
        var find    = await repositoryParameters.PosMeFindAutoIncrement();
        var codigo  = find.Value!;

        var numero  = Convert.ToInt32(codigo);
        numero      -= 1;
        find.Value  = numero.ToString();
        await repositoryParameters.PosMeUpdate(find);
        return Convert.ToInt32(codigo);
    }

    public async Task<string> GetValueParameter(string name, string valueDefault)
    {
        var parametro = await _repositoryParametersWeb.PosMeFindByKey(name);
        if (parametro is not null)
        {
            if (!string.IsNullOrEmpty(parametro.Value))
            {
                return parametro.Value;
            }
        }

        return valueDefault;
    }

    public async Task<string> GetCodigoFactura()
    {
        var find    = await repositoryParameters.PosMeFindCodigoFactura();
        var codigo  = find.Value!;

        if (codigo.IndexOf("-", StringComparison.Ordinal) < 0)
            throw new System.Exception(Mensajes.MnesajeCountadoDeAbonoMalFormado);

        var prefix  = find.Value!.Split("-")[0];
        var counter = find.Value!.Split("-")[1];
        var numero  = Convert.ToInt32(counter);
        numero      += 1;
        var nuevoCodigoFactura = prefix + "-" + Convert.ToString(numero).PadLeft(4, '0');
        find.Value  = nuevoCodigoFactura;
        await repositoryParameters.PosMeUpdate(find);

        return codigo;
    }

    private string GetFilePath(string filename)
    {
        var folderPath = Environment.GetFolderPath(DeviceInfo.Platform == DevicePlatform.Android
            ? Environment.SpecialFolder.LocalApplicationData
            : Environment.SpecialFolder.MyDocuments);

        return Path.Combine(folderPath, filename);
    }

    public async Task<string> GenerarUrlPago()
    {
        var nickname = "";
        var password = "";
        HttpClient httpClient = new();
        var nvc = new List<KeyValuePair<string, string>>
        {
            new("txtNickname", nickname),
            new("txtPassword", password)
        };
        var req = new HttpRequestMessage(HttpMethod.Post, Constantes.UrlRequestDownload)
        {
            Content = new FormUrlEncodedContent(nvc)
        };

        var response = await httpClient.SendAsync(req);
        if (!response.IsSuccessStatusCode)
        {
            return "";
        }
        else
        {
            return "";
        }
    }

    public async Task ZeroCounter()
    {
        var find    = await repositoryParameters.PosMeFindCounter();
        var value   = 0;
        find.Value  = $"{value}";
        await repositoryParameters.PosMeUpdate(find);
    }

    public async Task<string> FileImage(IScreenshotResult? screenshotResult, string fileName)
    {
        if (screenshotResult is null)
        {
            return "";
        }

        await using var stream  = await screenshotResult.OpenReadAsync();
        using var memoryStream  = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var filePath            = GetFilePath(fileName);
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return "";
        }

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        await File.WriteAllBytesAsync(filePath, memoryStream.ToArray());
        return filePath;
    }


    public string FormatString(string input)
    {
        // Si la cadena tiene menos de 12 caracteres, rellenar con ceros a la izquierda
        if (input.Length < 12)
        {
            return input.PadLeft(12, '0');
        }
        // Si la cadena tiene más de 12 caracteres, tomar los últimos 12 caracteres
        else if (input.Length > 12)
        {
            return input.Substring(input.Length - 12);
        }

        // Si la cadena tiene exactamente 12 caracteres, devolverla tal cual
        return input;
    }

    public string ConcatenatePairs(string input)
    {
        // Verificar que la longitud del input sea 8
        if (input.Length != 12)
        {
            throw new ArgumentException("La cadena debe tener exactamente 12 caracteres.");
        }

        string result = string.Empty;

        // Recorrer la cadena de 2 en 2
        for (int i = 0; i < input.Length; i += 2)
        {
            // Tomar dos caracteres a la vez
            string pair     = input.Substring(i, 2);
            char temporal   = ConvertToHexFormat(pair);

            // Concatenar el par al resultado
            result += temporal.ToString();
        }

        return result;
    }

    public char ConvertToHexFormat(string input)
    {
        // Convertir el valor string a entero
        int decimalValue    = int.Parse(input);

        // Convertir el valor entero a su representación hexadecimal
        char character      = (char)decimalValue;

        // Retornar el formato con "\x" al inicio
        return character;
    }

    public async Task<bool> ReordenarListaClientes()
    {
        
        //Escribir la posicion de los customer ordenados por el usuario
        var paramShare                          = await repositoryParameters.PosMeFindCustomerOrderCustomer();
        string temporal                         = "";
        List<CustomerOrderShare> customOrder    = [];
        if (!string.IsNullOrWhiteSpace(paramShare.Value))
        {
            customOrder = (JsonConvert.DeserializeObject<List<CustomerOrderShare>>(paramShare.Value))?.OrderBy(pl => pl.Position ).ToList() ?? [];
        }

        if (customOrder.Count > 0)
        {
            //Ordenar los que el usuario ordeno
            var findAllCustomerP = await _repositoryTbCustomer.PosMeFindAll();
            foreach (var customer in findAllCustomerP)
            {
                var cliente = customOrder.FirstOrDefault(c => c.EntityId == customer.EntityId);
                if (cliente is not null)
                {
                    customer.Secuencia = cliente.Position;
                }
                else
                {
                    customer.Secuencia = -1;
                }
            }

            //Aun que el usuario lo ahiga ordenado, si no son de el siempre van de ultimo
            var sequencia = findAllCustomerP.Count + 1;
            foreach (var customerResponse in findAllCustomerP.Where(customerResponse => customerResponse.Me == 0).OrderBy(k => k.FirstName))
            {   
                customerResponse.Secuencia = sequencia;
                sequencia++;
            }



            //rellenar las pocicones en 0, o nullas, con su secuenca correcta
            foreach(var customer in findAllCustomerP.Where(p => p.Secuencia == -1).OrderBy(p => p.FirstName))
            {
                var indexTemporal   = 0;
                for (var i = 0; i < findAllCustomerP.Count; i++)
                {
                    var cliente = findAllCustomerP.Where(p => p.Secuencia == i).FirstOrDefault();
                    if (cliente is null)
                    {
                        indexTemporal = i;
                        break;
                    }
                }
                customer.Secuencia  = indexTemporal;
            }

           

            //Resetear los valoes para que inicie en 0 de manera secuencial
            var index = 0;
            foreach (var cliente in findAllCustomerP.OrderBy(k => k.Secuencia))
            {
                cliente.Secuencia = index;
                index++;
            }

            
            await _repositoryTbCustomer.PosMeUpdateAll(findAllCustomerP);
            VariablesGlobales.OrdenarClientes = false;
        }
        else
        {
            

            // actualizar el campo sequena de todos los clintes
            var index               = 0;
            var findAllCustomerP    = await _repositoryTbCustomer.PosMeFindAll();
            foreach (var itemCustomer in findAllCustomerP.OrderByDescending(k=>k.Me).ThenBy(c => c.FirstName ))
            {   
                itemCustomer.Secuencia = index;
                index++;
            }

            await _repositoryTbCustomer.PosMeUpdateAll(findAllCustomerP);
            VariablesGlobales.OrdenarClientes = false;
           
        }

        return true;


    }

    public async Task<bool> ReordenarListaClientesFacturas()
    {

        //Escribir la posicion de los customer ordenados por el usuario
        var paramShare  = await repositoryParameters.PosMeFindCustomerOrderInvoice();
        string temporal = "";
        List<CustomerOrderShare> customOrder = [];
        if (!string.IsNullOrWhiteSpace(paramShare.Value))
        {
            customOrder = (JsonConvert.DeserializeObject<List<CustomerOrderShare>>(paramShare.Value))?.OrderBy(pl => pl.Position).ToList() ?? [];
        }

        if (customOrder.Count > 0)
        {
            //Ordenar los que el usuario ordeno
            var findAllCustomerP = await _repositoryTbCustomer.PosMeFindAll();
            foreach (var customer in findAllCustomerP)
            {
                var cliente = customOrder.FirstOrDefault(c => c.EntityId == customer.EntityId);
                if (cliente is not null)
                {
                    customer.Secuencia = cliente.Position;
                }
                else
                {
                    customer.Secuencia = -1;
                }
            }

            //Aun que el usuario lo ahiga ordenado, si no son de el siempre van de ultimo
            var sequencia = findAllCustomerP.Count + 1;
            foreach (var customerResponse in findAllCustomerP.Where(customerResponse => customerResponse.Me == 0).OrderBy(k => k.FirstName))
            {
                customerResponse.Secuencia = sequencia;
                sequencia++;
            }



            //rellenar las pocicones en 0, o nullas, con su secuenca correcta
            foreach (var customer in findAllCustomerP.Where(p => p.Secuencia == -1).OrderBy(p => p.FirstName))
            {
                var indexTemporal = 0;
                for (var i = 0; i < findAllCustomerP.Count; i++)
                {
                    var cliente = findAllCustomerP.Where(p => p.Secuencia == i).FirstOrDefault();
                    if (cliente is null)
                    {
                        indexTemporal = i;
                        break;
                    }
                }
                customer.Secuencia = indexTemporal;
            }



            //Resetear los valoes para que inicie en 0 de manera secuencial
            var index = 0;
            foreach (var cliente in findAllCustomerP.OrderBy(k => k.Secuencia))
            {
                cliente.Secuencia = index;
                index++;
            }


            await _repositoryTbCustomer.PosMeUpdateAll(findAllCustomerP);
            VariablesGlobales.OrdenarClientes = false;
        }
        else
        {


            // actualizar el campo sequena de todos los clintes
            var index = 0;
            var findAllCustomerP = await _repositoryTbCustomer.PosMeFindAll();
            foreach (var itemCustomer in findAllCustomerP.OrderByDescending(k => k.Me).ThenBy(c => c.FirstName))
            {
                itemCustomer.Secuencia = index;
                index++;
            }

            await _repositoryTbCustomer.PosMeUpdateAll(findAllCustomerP);
            VariablesGlobales.OrdenarClientes = false;

        }

        return true;

    }


    public async Task<List<Api_AppMobileApi_GetDataDownloadCustomerResponse>> ReordenarListaAbono(List<Api_AppMobileApi_GetDataDownloadCustomerResponse> listaBase)
    {
        List<Api_AppMobileApi_GetDataDownloadCustomerResponse> listaOrdenada;

        //ordenar.
        //marcar la posicion de los que lestoca cobrar y tienene posicion asignada
        var paramShare                          = await repositoryParameters.PosMeFindCustomerOrderShare();
        List<CustomerOrderShare> customOrder    = [];
        if (!string.IsNullOrWhiteSpace(paramShare.Value))
        {
            customOrder = JsonConvert.DeserializeObject<List<CustomerOrderShare>>(paramShare.Value) ?? [];
        }

        if (customOrder.Count > 0)
        {
            foreach (var customer in listaBase)
            {
                var cliente = customOrder.FirstOrDefault(c => c.EntityId == customer.EntityId);
                if (cliente is not null)
                {
                    customer.SecuenciaAbono = cliente.Position;
                }
                else
                {
                    customer.SecuenciaAbono = -1;
                }
            }
        }


        //dejar de ultimo los que no les toca pago
        //var secuenciaAbono = listaBase.Count + 1;
        //foreach (var customerResponse in listaBase.Where(customerResponse => customerResponse.FirstBalanceDate.Date > DateTime.Today))
        //{
        //    customerResponse.SecuenciaAbono = secuenciaAbono;
        //    secuenciaAbono++;
        //}


        //rellenar las pocicones en 0, o nullas, con su secuenca correcta
        for (var i= 0 ; i < listaBase.Count; i++)
        {
            var cliente = listaBase.Where(p => p.SecuenciaAbono == i).FirstOrDefault();
            if (cliente is null)
            {
                var customerLibre = listaBase.Where(p => p.SecuenciaAbono == -1 ).FirstOrDefault();
                if (customerLibre is not null)
                {
                    customerLibre.SecuenciaAbono = i;
                }
            }
        }

        //Resetear los valoes para que inicie en 0 de manera secuencial
        var index = 0;
        foreach (var cliente in listaBase.OrderBy(k => k.SecuenciaAbono))
        {
            cliente.SecuenciaAbono = index;
            index++;
        }


        //actualizar el campo sequena de todos los clintes
        var findAllCustomer = await _repositoryTbCustomer.PosMeFindAll();
        foreach ( var itemCustomer in findAllCustomer)
        {
            var customer_ = listaBase.Where(p => p.EntityId == itemCustomer.EntityId).FirstOrDefault();
            if(customer_ is not null)
            {
                itemCustomer.SecuenciaAbono = customer_.SecuenciaAbono;
            }
            else
            {
                itemCustomer.SecuenciaAbono = -1;
            }
        }

        await _repositoryTbCustomer.PosMeUpdateAll(findAllCustomer);
        VariablesGlobales.OrdenarAbonos = false;
        listaOrdenada                   = listaBase
            .OrderBy(x => x.SecuenciaAbono)
            .ToList();
        
        return listaOrdenada;
    }
}