using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface IUserService
    {
        Task<AuthenticationResponse> RegisterAsync(RegisterRequest registerRequest);
        Task<(ConfirmationEmailStatus, string)> ConfirmEmail(string confirmationToken);
        AuthenticationResponse Authenticate(AuthenticationRequest credentials);
        string GetToken(AuthenticationRequest credentials);
        AuthenticationResponse RefreshToken(string refreshToken);
        ConfirmationEmailStatus ResendEmail(string confirmationToken);
    }
}
