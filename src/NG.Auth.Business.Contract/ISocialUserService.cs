using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using NG.DBManager.Infrastructure.Contracts.Models;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface ISocialUserService
    {
        Task<SocialUser> RegisterAsync(SocialRegisterRequest registerRequest);
        SocialUser Authenticate(SocialAuthenticationRequest credentials);
    }
}
