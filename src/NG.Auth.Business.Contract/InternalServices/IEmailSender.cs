namespace NG.Auth.Business.Contract.InternalServices
{
    public interface IEmailSender
    {
        void SendConfirmationEmail(string toName, string toEmail, string refreshToken, string accessToken);
        void SendPasswordRecoveryEmail(string toName, string toEmail, string token);
    }
}
