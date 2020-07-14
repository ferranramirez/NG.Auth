using NG.Auth.Domain;
using NG.DBManager.Infrastructure.Contracts.Models;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface IUserService
    {
        Task<User> RegisterAsync(RegisterRequest registerRequest);
        AuthenticationResponse Authenticate(AuthenticationRequest credentials);
        AuthenticationResponse RefreshToken(string refreshToken);
    }
}
