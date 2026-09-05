using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public class RepositoryTbLogs(DataBase dataBase) : RepositoryFacade<TbLogs>(dataBase), IRepositoryTbLogs
{
    public Task<int> PosMeInsertLog(string severity, string logs)
    {
        var log = new TbLogs
        {
            Fecha = DateTime.Now,
            Severity = severity,
            Logs = logs
        };
        return dataBase.Database.InsertAsync(log);
    }

    public Task<List<TbLogs>> PosMeFindAllDescending()
    {
        return dataBase.Database.Table<TbLogs>()
            .OrderByDescending(log => log.LogId)
            .Take(300)
            .ToListAsync();
    }
}
