using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace v4posme_maui.Services.SystemNames
{
    public enum TypeTransaction
    {
        TransactionInvoiceBilling = 19,
        TransactionShare = 23,
        TransactionQueryMedical = 35
    }

    public enum TypeQueryMedical
    {
        Entrada = 518,
        Salida = 519,
        ConsultaMedica = 716,
        Visita = 739
    }


    public enum TypePeriodPay
    {
        Mensual = 190,
        Quincenal = 189,
        Semanal = 188,
        Diario = 531,
        Catorcenal = 2322,
        MesYMedio = 203,
    }
    public enum TypeCurrency
    {
        Cordoba = 1,
        Dolar = 2,
    }
    public enum TypeTransactionCausal {
        Contado = 21,
        Credito = 22
    }
    public enum  TypeComponent
    {
        Itme = 33,
        Customer = 36
    }

    public enum TypePayment
    {
        TarjetaDebito=1,
        TarjetaCredito,
        Efectivo,
        Cheque,
        Monedero,
        Otros
    }

    public enum TypePermission
    {
        Selected = 0,
        Inserted = 1,
        Deleted  = 2,
        Updated  = 3
    }
    public enum TypeImpact
    {
        None = -1, /*no puede ver ningun registro*/
        All = 0, /*puede ver todos los registros*/
        Branch = 1, /*puede ver solo los registros de la sucursal*/
        Me=2, /*puede ver solo los que yo cree*/
    }
    public enum TypeMenuElement
    {
        Left = 5
    }
    public enum TypeMenuElementID
    {
        app_inventory_item_index_aspx = 55 
    }
}
