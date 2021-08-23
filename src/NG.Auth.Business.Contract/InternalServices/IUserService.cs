using NG.Auth.Domain;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;

namespace NG.Auth.Business.Contract.InternalServices
{
    public interface IUserService
    {
        AuthenticationResponse GetAuthenticationResponse(AuthorizedUser authUser);
        string GetAccessToken(AuthorizedUser authUser);
        User GetExistingUser(CommonRegisterFields registerRequest);
    }
}
