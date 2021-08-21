using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface ISocialUserService
    {
        Task<AuthenticationResponse> RegisterAsync(SocialRegisterRequest registerRequest);
        AuthenticationResponse Authenticate(SocialAuthenticationRequest credentials);
    }
}
