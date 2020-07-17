using NG.Auth.Domain;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface IUserService
    {
        Task<AuthenticationResponse> RegisterAsync(RegisterRequest registerRequest);
        AuthenticationResponse Authenticate(AuthenticationRequest credentials);
        AuthenticationResponse RefreshToken(string refreshToken);
    }
}
