using NG.Auth.Domain;
using NG.DBManager.Infrastructure.Contracts.Models;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface IUserService
    {
        Task<bool> Register(User user);
        string Authenticate(Credentials credentials);
    }
}
