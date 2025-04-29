using Unity;
using v4posme_maui.Models;
using v4posme_maui.Services.Helpers;
using v4posme_maui.Services.SystemNames;


namespace v4posme_maui.Services.Repository;

public class RepositoryTbUser(DataBase dataBase, HelperCore helperCore) : RepositoryFacade<Api_CoreAccount_LoginMobileObjUserResponse>(dataBase),IRepositoryTbUser
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

    public async Task<Api_CoreAccount_LoginMobileObjUserResponse?> PosMeFindUserByNicknameAndPassword(string nickname, string password)
    {
        return await dataBase.Database.Table<Api_CoreAccount_LoginMobileObjUserResponse>()
            .Where(user => user.Nickname == nickname && user.Password == password)
            .FirstOrDefaultAsync();
    }

    public async Task<int> PosMeRowCount()
    {
        return await dataBase.Database.Table<Api_CoreAccount_LoginMobileObjUserResponse>().CountAsync();
    }

    public async Task<bool> PosMeValidateUser(Api_CoreAccount_LoginMobileObjUserResponse user1, Api_CoreAccount_LoginMobileObjUserResponse user2)
    {
        var counter = await helperCore.GetCounter();
        if (counter <= 0)
        {
            return false;
        }
       
        if (string.IsNullOrEmpty(user1.Nickname) || string.IsNullOrEmpty(user1.Company) ||
            string.IsNullOrEmpty(user2.Nickname) || string.IsNullOrEmpty(user2.Company))
        {
            return false;
        }

        var company1 = helperCore.ExtractCompanyKey(user1.Company);
        var company2 = helperCore.ExtractCompanyKey(user2.Company);

        var validate1 = $"{user1.Nickname.ToLowerInvariant()}{company1.ToLowerInvariant()}";
        var validate2 = $"{user2.Nickname.ToLowerInvariant()}{company2.ToLowerInvariant()}";

        return string.CompareOrdinal(validate1, validate2) != 0;
    }

}