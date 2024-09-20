using v4posme_maui.Models;


namespace v4posme_maui.Services.Repository;

public class RepositoryTbUser(DataBase dataBase) : RepositoryFacade<Api_CoreAccount_LoginMobileObjUserResponse>(dataBase),IRepositoryTbUser
{

    public async Task PosMeOnRemember()
    {
        var listAsync = await dataBase.Database.Table<Api_CoreAccount_LoginMobileObjUserResponse>().ToListAsync();
        if (listAsync.Count <= 0)
        {
            return;
        }

        foreach (var user in listAsync)
        {
            user.Remember = false;
        }

        await dataBase.Database.UpdateAllAsync(listAsync);
    }

    public async Task<Api_CoreAccount_LoginMobileObjUserResponse?> PosmeFindUserRemember()
    {
        return await dataBase.Database.Table<Api_CoreAccount_LoginMobileObjUserResponse>()
            .Where(user => user.Remember)
            .FirstOrDefaultAsync();
    }

    public async Task<Api_CoreAccount_LoginMobileObjUserResponse?> PosMeFindUserByNicknameAndPassword(string nickname,
        string password)
    {
        return await dataBase.Database.Table<Api_CoreAccount_LoginMobileObjUserResponse>()
            .Where(user => user.Nickname == nickname && user.Password == password)
            .FirstOrDefaultAsync();
    }

    public async Task<int> PosMeRowCount()
    {
        return await dataBase.Database.Table<Api_CoreAccount_LoginMobileObjUserResponse>().CountAsync();
    }
}