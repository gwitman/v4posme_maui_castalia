using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;
using v4posme_maui.Services.Api;
namespace v4posme_maui.Services.Repository;

public class RepositoryTbParameterSystem(DataBase dataBase) : RepositoryFacade<TbParameterSystem>(dataBase), IRepositoryTbParameterSystem
{
    public Task<TbParameterSystem> PosMeFindLogo()
    {
        return dataBase.Database.Table<TbParameterSystem>()
            .FirstOrDefaultAsync(system => system.Name == Constantes.ParametroLogo);
    }

    public Task<TbParameterSystem> PosMeFindCounter()
    {
        return dataBase.Database.Table<TbParameterSystem>()
            .FirstOrDefaultAsync(system => system.Name == Constantes.ParametroCounter);
    }
    public Task<TbParameterSystem> PosMeFindByName(string name )
    {
        return dataBase.Database.Table<TbParameterSystem>()
            .FirstOrDefaultAsync(system => system.Name == name);
    }

    
    public Task<TbParameterSystem> PosMeFindPrinter()
    {
        return dataBase.Database.Table<TbParameterSystem>()
            .FirstOrDefaultAsync(system => system.Name == Constantes.ParametroPrinter);
    }

    public Task<TbParameterSystem> PosMeFindCodigoAbono()
    {
        return dataBase.Database.Table<TbParameterSystem>()
            .FirstOrDefaultAsync(system => system.Name == Constantes.ParametroCodigoAbono);
    }

    public Task<TbParameterSystem> PosMeFindCodigoFactura()
    {
        return dataBase.Database.Table<TbParameterSystem>()
            .FirstOrDefaultAsync(system => system.Name == Constantes.ParameterCodigoFactura);
    }

    public Task<TbParameterSystem> PosMeFindAutoIncrement()
    {
        return dataBase.Database.Table<TbParameterSystem>()
            .FirstOrDefaultAsync(system => system.Name == Constantes.ParemeterEntityIDAutoIncrement);
    }

    //Carlos Conto
	public Task<TbParameterSystem> PosMeFindCodigoVisita()
	{ //ParameterCodigoVisita
		return dataBase.Database.Table<TbParameterSystem>()
			.FirstOrDefaultAsync(system => system.Name == Constantes.ParameterCodigoVisita);
	}
}