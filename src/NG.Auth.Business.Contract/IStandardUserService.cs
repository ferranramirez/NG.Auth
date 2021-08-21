using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface IStandardUserService
    {
        Task<AuthenticationResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<ConfirmationEmailStatus> ConfirmEmail(string confirmationToken, string accessToken);
        AuthenticationResponse Authenticate(StandardAuthenticationRequest credentials);
        string GetToken(StandardAuthenticationRequest credentials);
        AuthenticationResponse RefreshToken(string refreshToken);
        Task<bool> UpdatePassword(string token, string password);
    }
}
