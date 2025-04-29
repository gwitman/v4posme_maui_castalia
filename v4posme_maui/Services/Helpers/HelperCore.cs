using DevExpress.Utils.Filtering;
using v4posme_maui.Models;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.Services.Helpers;

public class HelperCore(
        IRepositoryTbParameterSystem repositoryParameters, IRepositoryParameters _repositoryParametersWeb
)
{
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
        var find = await repositoryParameters.PosMeFindCodigoAbono();
        var codigo = find.Value!;

        if (codigo.IndexOf("-", StringComparison.Ordinal) < 0)
            throw new Exception(Mensajes.MnesajeCountadoDeAbonoMalFormado);

        var prefix = find.Value!.Split("-")[0];
        var counter = find.Value!.Split("-")[1];
        var numero = Convert.ToInt32(counter);
        numero += 1;
        var nuevoCodigoAbono = prefix + "-" + Convert.ToString(numero).PadLeft(4, '0');
        find.Value = nuevoCodigoAbono;
        await repositoryParameters.PosMeUpdate(find);

        return codigo;
    }

	public async Task<string> GetCodigoVisita()
	{
		var find = await repositoryParameters.PosMeFindCodigoVisita();
		var codigo = find.Value!;

		if (codigo.IndexOf("-", StringComparison.Ordinal) < 0)
        {
			throw new Exception(Mensajes.MensajeCountadorDeVisitaMalFormado);
		}

		var prefix = find.Value!.Split("-")[0];
		var counter = find.Value!.Split("-")[1];
		var numero = Convert.ToInt32(counter);
		numero += 1;
		var nuevoCodigoAbono = prefix + "-" + Convert.ToString(numero).PadLeft(4, '0');
		find.Value = nuevoCodigoAbono;
		await repositoryParameters.PosMeUpdate(find);

		return codigo;
	}

	public async Task<int> GetAutoIncrement()
    {
        var find = await repositoryParameters.PosMeFindAutoIncrement();
        var codigo = find.Value!;
        
        var numero = Convert.ToInt32(codigo);
        numero -= 1;
        find.Value = numero.ToString();
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
        var find = await repositoryParameters.PosMeFindCodigoFactura();
        var codigo = find.Value!;

        if (codigo.IndexOf("-", StringComparison.Ordinal) < 0)
            throw new Exception(Mensajes.MnesajeCountadoDeAbonoMalFormado);

        var prefix = find.Value!.Split("-")[0];
        var counter = find.Value!.Split("-")[1];
        var numero = Convert.ToInt32(counter);
        numero += 1;
        var nuevoCodigoFactura = prefix + "-" + Convert.ToString(numero).PadLeft(4, '0');
        find.Value = nuevoCodigoFactura;
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
        var find = await repositoryParameters.PosMeFindCounter();
        var value = 0;
        find.Value = $"{value}";
        await repositoryParameters.PosMeUpdate(find);
    }
    
    public async Task<string> FileImage(IScreenshotResult? screenshotResult, string fileName)
    {
        if (screenshotResult is null)
        {
            return "";
        }

        await using var stream = await screenshotResult.OpenReadAsync();
        using var memoryStream = new MemoryStream();
        await stream.CopyToAsync(memoryStream);
        var filePath = GetFilePath(fileName);
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


    public  string FormatString(string input)
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

    public  string ConcatenatePairs(string input)
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
            string pair = input.Substring(i, 2);
            char temporal = ConvertToHexFormat(pair);

            // Concatenar el par al resultado
            result += temporal.ToString();
        }

        return result;
    }

    public  char ConvertToHexFormat(string input)
    {
        // Convertir el valor string a entero
        int decimalValue = int.Parse(input);

        // Convertir el valor entero a su representación hexadecimal
        char character = (char)decimalValue;

        // Retornar el formato con "\x" al inicio
        return character;
    }

    public List<Api_AppMobileApi_GetDataDownloadCustomerResponse> ReordenarLista(List<Api_AppMobileApi_GetDataDownloadCustomerResponse> listaBase, List<Api_AppMobileApi_GetDataDownloadCustomerResponse> listSecundaria, bool porFecha = false)
    {
        // Paso 1: Aplicar los cambios de secuencia
        foreach (var cambio in listSecundaria)
        {
            var cliente = listaBase.FirstOrDefault(c => c.EntityId == cambio.EntityId);
            if (cliente != null)
            {
                cliente.Secuencia = cambio.Secuencia;
            }
        }

        // Paso 2: Ordenar por secuencia y luego por EntityId para desempate
        List<Api_AppMobileApi_GetDataDownloadCustomerResponse> listaOrdenada;
        if (porFecha)
        {
            listaOrdenada = listaBase
                .OrderBy(c => c.FirstBalanceDate)
                .ThenBy(c => c.Secuencia)
                .ThenBy(c => c.EntityId)
                .ToList();
        }
        else
        {
            listaOrdenada = listaBase
                .OrderBy(c => c.Secuencia)
                .ThenBy(c => c.EntityId)
                .ToList(); 
        }
        

        // Paso 3: Reasignar secuencias para que sean consecutivas
        var nuevaSecuencia = 1;
        foreach (var cliente in listaOrdenada)
        {
            cliente.Secuencia = nuevaSecuencia++;
        }

        return listaOrdenada;
    }
}