﻿using System.Diagnostics;
using System.Windows.Input;
using DevExpress.Maui.Scheduler;
using Plugin.BLE;
using SkiaSharp;
using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.HelpersPrinters;
using v4posme_maui.Services.HelpersPrinters.Interfaces.Command;
using v4posme_maui.Services.Repository;
using v4posme_maui.Services.SystemNames;

namespace v4posme_maui.ViewModels.More.ProductosRetornos;

public class ProductosRetornosViewModel : BaseViewModel
{
    private readonly IRepositoryItems _repositoryItems;
    private readonly IRepositoryTbParameterSystem _parameterSystem;
    private readonly IRepositoryParameters _repositoryParameters;


    public ProductosRetornosViewModel()
    {
        _repositoryItems        = VariablesGlobales.UnityContainer.Resolve<IRepositoryItems>();
        _parameterSystem        = VariablesGlobales.UnityContainer.Resolve<IRepositoryTbParameterSystem>();
        _repositoryParameters   = VariablesGlobales.UnityContainer.Resolve<IRepositoryParameters>();
        _items                  = new();
        PrintCommand            = new Command(OnPrintCommand);
    }
    
    public ICommand PrintCommand { get; private set; }
    private DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> _items;
    public DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse> Items
    {
        get=> _items; 
        set=> SetProperty(ref _items,value);
    }
    
    private Api_AppMobileApi_GetDataDownloadParametersResponse? _companyTelefono;

    public Api_AppMobileApi_GetDataDownloadParametersResponse? CompanyTelefono
    {
        get => _companyTelefono;
        private set => SetProperty(ref _companyTelefono, value);
    }

    private Api_AppMobileApi_GetDataDownloadParametersResponse? _companyRuc;

    public Api_AppMobileApi_GetDataDownloadParametersResponse? CompanyRuc
    {
        get => _companyRuc;
        private set => SetProperty(ref _companyRuc, value);
    }
    
    private async void OnPrintCommand()
    {
        try
        {
            var parametroPrinter    = await _parameterSystem.PosMeFindPrinter();
            var logo                = await _parameterSystem.PosMeFindLogo();
            if (string.IsNullOrWhiteSpace(parametroPrinter.Value))
            {
                return;
            }

            var printer = new Printer(parametroPrinter.Value);
            if (!CrossBluetoothLE.Current.IsOn)
            {
                ShowMensajePopUp(Mensajes.MensajeBluetoothState);
                return;
            }

            IsBusy = true;
            if (!string.IsNullOrWhiteSpace(logo.Value))
            {
                var readImage = Convert.FromBase64String(logo.Value!);
                printer.AlignRight();
                printer.Image(SKBitmap.Decode(readImage));
            }

            var user = VariablesGlobales.User;
            if (user is null)
            {
                ShowMensajePopUp(Mensajes.UsuarioNoExiste);
                return;
            }
            if (Company is null)
            {
                ShowMensajePopUp(Mensajes.CompanyNoExiste);
                return;
            }
            printer.AlignCenter();
            printer.BoldMode(Company.Name!);
            printer.BoldMode($"RUC: {CompanyRuc!.Value}");
            printer.BoldMode("Productos con Remanentes");
            printer.BoldMode(DateTime.Now.ToString("yyyy-MM-dd hh:mm tt"));
            printer.NewLine();
            printer.AlignLeft();
            printer.Append($"El usuario {user.Nickname} a generado el reporte de productos con cantidades distintos de 0");
            printer.NewLine();
            if (Items.Count > 0)
            {
                printer.Append("Detalle de productos");
                printer.Append("Descripción     Barra      Qt");
                printer.NewLine();                
                foreach (var item in Items)
                {
                    var barCode = item.BarCode;
                    barCode     = barCode.Split(",")[0];
                    
                    if (barCode.Length  < 12)
                        barCode = barCode.PadLeft(12, '0'); // Rellena con ceros a la izquierda
                    else
                        barCode = barCode.Substring(0, 12); // Corta a 12 caracteres

                    printer.Append($"{item.Name}");
                    printer.Append($"{barCode}               {item.Quantity:N2}");
                    printer.Append("..............................");
                    
                }
            }
            printer.NewLine();
            printer.Append($"TOTAL ARTICULOS: {Items.Count}");
            printer.NewLine();
            printer.AlignCenter();
            printer.Append(CompanyTelefono!.Value!);
            printer.Append(Company.Address!);
            printer.FullPaperCut();
            printer.Print();
            if (printer.Device is null)
            {
                ShowMensajePopUp(Mensajes.MensajeDispositivoNoConectado);
            }

            IsBusy = false;
        }
        catch (Exception e)
        {
            ShowMensajePopUp(e.Message);
        }
    }

    
    public async void LoadData()
    {
        try
        {
            var data        = await _repositoryItems.PosMeQuantityDistintoZero();
            Items           = new DXObservableCollection<Api_AppMobileApi_GetDataDownloadItemsResponse>(data);
            CompanyTelefono = await _repositoryParameters.PosMeFindByKey("CORE_PHONE");
            CompanyRuc      = await _repositoryParameters.PosMeFindByKey("CORE_COMPANY_IDENTIFIER");
            Company         = VariablesGlobales.TbCompany;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            ShowMensajePopUp(e.Message);
        }
    }
}