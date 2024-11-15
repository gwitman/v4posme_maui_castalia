using v4posme_maui.Services.Repository;
using System.Runtime.Intrinsics.Arm;
using v4posme_maui.Models;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using GoogleGson;
using Android.Service.Autofill;
using Android.Widget;
using Android.Util;
using System.Runtime.CompilerServices;
namespace v4posme_maui.Services.Helpers;

public class HelperCore(IRepositoryTbParameterSystem repositoryParameters)
{
   
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

    public string GetFilePath(string filename)
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
}