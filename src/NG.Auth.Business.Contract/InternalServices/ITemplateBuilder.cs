using MimeKit;

namespace NG.Auth.Business.Contract.InternalServices
{
    public interface ITemplateBuilder
    {
        void ReplaceTags(ref BodyBuilder builder, string tag, string replacementText);
        void BuildDefaultEmail(string toName, string toEmail, string FromEmail, string fromName, string subject,
            string filePath, out MimeMessage message, out BodyBuilder builder, out string urlBase);
        BodyBuilder OpenFile(string filePath);
    }
}
