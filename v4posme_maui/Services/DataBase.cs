﻿using v4posme_maui.Models;
using SQLite;
using v4posme_maui.Services.SystemNames;

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
                new() { Name = Constantes.ParametroCounter, Description = "Contador Global", Value = "0" },
                new() { Name = Constantes.ParametroLogo, Description = "Logo de la aplicación", Value = "" },
                new() { Name = Constantes.ParametroPrinter, Description = "Impresora", Value = "Printer" },
                new() { Name = Constantes.ParametroCodigoAbono, Description = "Número de abono", Value = "ABO-0001" },
                new() { Name = Constantes.ParameterCodigoFactura, Description = "Número de factura", Value = "FAC-0001" },
                new() { Name = Constantes.ParemeterEntityIDAutoIncrement, Description = "Auto incrementado", Value = "-1" },
				new() { Name = Constantes.ParameterCodigoVisita, Description = "Número de visita", Value = "VST-0001" },
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