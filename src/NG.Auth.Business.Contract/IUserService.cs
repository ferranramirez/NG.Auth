using NG.Auth.Domain;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface IUserService
    {
        Task<bool> RegisterAsync(UserDto userDto);
        string Authenticate(Credentials credentials);
    }
}
