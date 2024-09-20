using v4posme_maui.Services.Repository;
using System.Runtime.Intrinsics.Arm;
using v4posme_maui.Models;
using System.Reflection.Metadata;
using Unity;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;

namespace v4posme_maui.Services.Helpers;

class HelperCustomerCreditDocumentAmortization
{
    public static async Task<string> ApplyShare(int entityId, string invoiceNumber, decimal amountApply)
    {
        var repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        var repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
        var repositoryTbCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();


        string resultado = "";

        var objCustomDocumentAmortization = await repositoryDocumentCreditAmortization.PosMeFilterByDocumentNumber(invoiceNumber);
        var objCustomerDocument = await repositoryDocumentCredit.PosMeFindDocumentNumber(invoiceNumber);
        var objCustomerResponse = await repositoryTbCustomer.PosMeFindEntityId(entityId);


        var tmpListaSave = new List<Api_AppMobileApi_GetDataDownloadDocumentCreditAmortizationResponse>();
        var amountApplyBackup = amountApply;
        var length = objCustomDocumentAmortization.Count;
        var aux = 0;
        //Actualiar Tabla de Amortiation
        foreach (var documentCreditAmortization in objCustomDocumentAmortization)
        {
            if (decimal.Compare(amountApply, decimal.Zero) <= 0)
            {
                break;
            }

            if (decimal.Compare(documentCreditAmortization.Remaining, amountApply) <= 0)
            {
                if (string.IsNullOrWhiteSpace(resultado))
                {
                    resultado += $"{documentCreditAmortization.CreditAmortizationId}:{amountApply}";
                }
                else
                {
                    resultado += $",{documentCreditAmortization.CreditAmortizationId}:{amountApply}";
                }

                amountApply = decimal.Subtract(amountApply, documentCreditAmortization.Remaining);
                documentCreditAmortization.Remaining = decimal.Zero;
                
            }
            else
            {
                if (string.IsNullOrWhiteSpace(resultado))
                {
                    resultado += $"{documentCreditAmortization.CreditAmortizationId}:{amountApply}";
                }
                else
                {
                    resultado += $",{documentCreditAmortization.CreditAmortizationId}:{amountApply}";
                }

                documentCreditAmortization.Remaining = decimal.Subtract(documentCreditAmortization.Remaining, amountApply);                
                amountApply = decimal.Zero;
            }

            tmpListaSave.Add(documentCreditAmortization);
        }

        //Actualizar Documento
        objCustomerDocument.Remaining -= amountApplyBackup;


        //Actulizar Saldo del Cliente 
        if (objCustomerResponse.CurrencyId == objCustomerDocument.CurrencyId)
        {
            objCustomerResponse.Balance -= amountApplyBackup;
        }
        //Actualiar Saldo del cliente Linea de Credito en Dolares y Documento esta en cordoba
        else if (objCustomerDocument.CurrencyId == (int)TypeCurrency.Cordoba && objCustomerDocument.CurrencyId == (int)TypeCurrency.Dolar)
        {
            objCustomerResponse.Balance -= decimal.Round(amountApplyBackup / objCustomerDocument.ExchangeRate, 2);
        }
        //Actualiar Saldo del cliente Linea de Credito en Cordoba y Documento esta en Dolares
        else if (objCustomerDocument.CurrencyId == (int)TypeCurrency.Dolar && objCustomerDocument.CurrencyId == (int)TypeCurrency.Cordoba)
        {
            objCustomerResponse.Balance -= decimal.Round(amountApplyBackup * objCustomerDocument.ExchangeRate, 2);
        }


        await repositoryDocumentCreditAmortization.PosMeUpdateAll(tmpListaSave);
        await repositoryDocumentCredit.PosMeUpdate(objCustomerDocument);
        await repositoryTbCustomer.PosMeUpdate(objCustomerResponse);

        return resultado;
    }

    public static async Task AnularAbono(string transactionNumber)
    {
        var repositoryCustomer = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbCustomer>();
        var repositoryDocumentCredit = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCredit>();
        var repositoryDocumentCreditAmortization = VariablesGlobales.UnityContainer.Resolve<IRepositoryDocumentCreditAmortization>();
        var repositoryTransactionMaster = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbTransactionMaster>();

        var findTransactionMaster = await repositoryTransactionMaster.PosMeFindByTransactionNumber(transactionNumber);
        var objCustomerResponse = await repositoryCustomer.PosMeFindEntityId(findTransactionMaster.EntityId);


        /*
         * Buscamos los documentos afectados que estan en referencia1
         */
        var documentNumbers = new List<string>();
        var idDocumentAmortization = findTransactionMaster.Reference1!.Split(",");
        foreach (var referencia in idDocumentAmortization)
        {
            var iddocument = referencia.Split(":")[0];
            var monto = decimal.Parse(referencia.Split(":")[1]);
            var documentCreditAmortization = await repositoryDocumentCreditAmortization.PosMeFindByAmortizationId(int.Parse(iddocument));
            if (documentNumbers.Count <= 0)
            {
                documentNumbers.Add(documentCreditAmortization.DocumentNumber!);
            }
            else if (!documentNumbers.Contains(documentCreditAmortization.DocumentNumber!))
            {
                documentNumbers.Add(documentCreditAmortization.DocumentNumber!);
            }
            monto = decimal.Add(monto, documentCreditAmortization.Remaining);
            documentCreditAmortization.Remaining = monto;
            await repositoryDocumentCreditAmortization.PosMeUpdate(documentCreditAmortization);
            
        }

        foreach (var number in documentNumbers)
        {
            var objCustomerDocument = await repositoryDocumentCredit.PosMeFindDocumentNumber(number);
            var amountApplyBackup = findTransactionMaster.SubAmount;
        
            //Actulizar Saldo del Cliente 
            if (objCustomerResponse.CurrencyId == objCustomerDocument.CurrencyId)
            {
                objCustomerResponse.Balance += amountApplyBackup;
            }
            //Actualiar Saldo del cliente Linea de Credito en Dolares y Documento esta en cordoba
            else if (objCustomerDocument.CurrencyId == (int)TypeCurrency.Cordoba && objCustomerDocument.CurrencyId == (int)TypeCurrency.Dolar)
            {
                objCustomerResponse.Balance += decimal.Round(amountApplyBackup / objCustomerDocument.ExchangeRate, 2);
            }
            //Actualiar Saldo del cliente Linea de Credito en Cordoba y Documento esta en Dolares
            else if (objCustomerDocument.CurrencyId == (int)TypeCurrency.Dolar && objCustomerDocument.CurrencyId == (int)TypeCurrency.Cordoba)
            {
                objCustomerResponse.Balance += decimal.Round(amountApplyBackup * objCustomerDocument.ExchangeRate, 2);
            }


            objCustomerDocument.Remaining = decimal.Add(objCustomerDocument.Remaining, amountApplyBackup);            
            await repositoryDocumentCredit.PosMeUpdate(objCustomerDocument);
            await repositoryCustomer.PosMeUpdate(objCustomerResponse);
        }
        await repositoryTransactionMaster.PosMeDelete(findTransactionMaster);
    }
}