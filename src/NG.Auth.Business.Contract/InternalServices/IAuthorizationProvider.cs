using NG.DBManager.Infrastructure.Contracts.Models;

namespace NG.Auth.Business.Contract.InternalServices
{
    public interface IAuthorizationProvider
    {
        string GetToken(User user);
    }
}
