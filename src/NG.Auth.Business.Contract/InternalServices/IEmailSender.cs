namespace NG.Auth.Business.Contract.InternalServices
{
    public interface IEmailSender
    {
        void SendEmailConfirmation(string toName, string toEmail, string token);
    }
}
