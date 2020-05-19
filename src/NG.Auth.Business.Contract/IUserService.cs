using NG.Auth.Domain;
using NG.DBManager.Infrastructure.Contracts.Models;

namespace NG.Auth.Business.Contract
{
    public interface IUserService
    {
        bool Register(User user);
        string Authenticate(Credentials credentials);
    }
}
