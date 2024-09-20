using v4posme_maui.Models;

namespace v4posme_maui.Services.Repository;

public interface IRepositoryTbUser : IRepositoryFacade<Api_CoreAccount_LoginMobileObjUserResponse>
{

    Task PosMeOnRemember();

    Task<Api_CoreAccount_LoginMobileObjUserResponse?> PosmeFindUserRemember();

    Task<Api_CoreAccount_LoginMobileObjUserResponse?> PosMeFindUserByNicknameAndPassword(string nickname,
        string password);

    Task<int> PosMeRowCount();
}