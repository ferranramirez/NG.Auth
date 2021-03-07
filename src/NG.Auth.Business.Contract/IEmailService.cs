using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using NG.DBManager.Infrastructure.Contracts.Models;
using System.Threading.Tasks;

namespace NG.Auth.Business.Contract
{
    public interface IEmailService
    {
        ConfirmationEmailStatus ResendConfirmationEmail(string AccessToken);
        bool SendPasswordRecoveryEmail(string email);
        User GetUser(string changePasswordToken);
    }
}
