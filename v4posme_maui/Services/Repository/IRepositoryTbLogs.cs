using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbLogs : IRepositoryFacade<TbLogs>
{
    Task<int> PosMeInsertLog(string severity, string logs);
    Task<List<TbLogs>> PosMeFindAllDescending();
}
