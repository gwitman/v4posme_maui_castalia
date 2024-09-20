using v4posme_maui.Models;
using SQLite;

namespace v4posme_maui.Services;

public class DataBase
{
    public readonly SQLiteAsyncConnection Database;

    public DataBase()
    {
        Database ??= new SQLiteAsyncConnection(ConstantsSqlite.DatabasePath, ConstantsSqlite.Flags);
    }

    public async void Init()
    {
        await Database.CreateTableAsync<Api_CoreAccount_LoginMobileObjUserResponse>();
        await Database.CreateTableAsync<TbParameterSystem>();
        var countParameters = await Database.Table<TbParameterSystem>().CountAsync();
        if (countParameters==0)
        {
            var parametrosDefault = new List<TbParameterSystem>
            {
                new() { Name = "COUNTER", Description = "Contador Global", Value = "0" },
                new() { Name = "LOGO", Description = "Logo de la aplicación", Value = "" },                
                new() { Name = "PRINTER", Description = "Impresora", Value = "Printer" },
                new() { Name = "TRANSACTION_SHARE", Description = "Número de abono", Value = "ABO-0001" },
                new() { Name = "TRANSACTION_INVOICE", Description = "Número de factura", Value = "FAC-0001" }
            };
            await Database.InsertAllAsync(parametrosDefault);
        }
    }

    public async void InitDownloadTables()
    {
        await Database.CreateTableAsync<Api_AppMobileApi_GetDataDownloadCustomerResponse>();
        await Database.CreateTableAsync<Api_AppMobileApi_GetDataDownloadItemsResponse>();
        await Database.CreateTableAsync<Api_AppMobileApi_GetDataDownloadParametersResponse>();
        await Database.CreateTableAsync<Api_AppMobileApi_GetDataDownloadDocumentCreditResponse>();
        await Database.CreateTableAsync<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>();
        await Database.CreateTableAsync<TbTransactionMaster>();
        await Database.CreateTableAsync<TbTransactionMasterDetail>();
        await Database.CreateTableAsync<TbCompany>();
    }
}