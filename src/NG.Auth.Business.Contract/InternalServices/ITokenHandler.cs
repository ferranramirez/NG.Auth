using NG.Auth.Domain;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;

namespace NG.Auth.Business.Contract.InternalServices
{
    public interface ITokenHandler
    {
        User GetUser(AuthenticationRequest credentials);
        string GenerateRefreshToken(AuthorizedUser authorizedUser);
        void SaveRefreshTokenInCache(string refreshToken, AuthorizedUser authorizedUser);
        AuthenticationResponse Authenticate(string refreshToken);
        bool IsEmailConfirmed(string accessToken);
    }
}
