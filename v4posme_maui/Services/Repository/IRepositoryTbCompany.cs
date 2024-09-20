using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbCompany : IRepositoryFacade<TbCompany>
{
    
}

public class RepositoryTbCompany(DataBase dataBase) : RepositoryFacade<TbCompany>(dataBase),IRepositoryTbCompany
{
}