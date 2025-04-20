using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbParameterSystem : IRepositoryFacade<TbParameterSystem>
{
    Task<TbParameterSystem> PosMeFindLogo();
    Task<TbParameterSystem> PosMeFindCounter();    
    Task<TbParameterSystem> PosMeFindPrinter();
    Task<TbParameterSystem> PosMeFindCodigoAbono();
    Task<TbParameterSystem> PosMeFindCodigoFactura();
    Task<TbParameterSystem> PosMeFindAutoIncrement();
    Task<TbParameterSystem> PosMeFindByName(string name);
    Task<TbParameterSystem> PosMeFindCustomerOrderShare();
	//Carlos Conto
	Task<TbParameterSystem> PosMeFindCodigoVisita();
}