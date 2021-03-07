using MailKit.Security;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using MimeKit;
using NG.Auth.Business.Contract.InternalServices;
using NG.Common.Library.Exceptions;
using System;
using System.IO;
using System.Net.Mail;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;

namespace NG.Auth.Business.Impl.InternalServices
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;
        private readonly ITemplateBuilder _templateBuilder;

        public EmailSender(
            IConfiguration configuration,
            ITemplateBuilder templateBuilder)
        {
            _configuration = configuration;
            _templateBuilder = templateBuilder;
        }

        public void SendConfirmationEmail(string toName, string toEmail, string refreshToken, string accessToken)
        {
            ConfigureEmailSettings(out IConfigurationSection emailSettings, out string host, out int port,
                out string userName, out string password, out string FromEmail, out string fromName);

            var confirmationEmail = emailSettings.GetSection("ConfirmationEmail");
            var subject = confirmationEmail.GetSection("Subject").Value;

            var filePath = Path.Combine("Templates", "ConfirmationEmail.html");

            _templateBuilder.BuildDefaultEmail(toName, toEmail, FromEmail, fromName, subject, filePath,
                out MimeMessage message, out BodyBuilder builder, out string urlBase);

            _templateBuilder.ReplaceTags(ref builder, "#{validationLink}#",
                string.Concat(urlBase, "/User?ConfirmationToken=", refreshToken, "&&AccessToken=", accessToken));

            _ = SendMessage(host, port, userName, password, message, builder);
        }

        public void SendPasswordRecoveryEmail(string toName, string toEmail, string token)
        {
            ConfigureEmailSettings(out IConfigurationSection emailSettings, out string host, out int port,
                out string userName, out string password, out string FromEmail, out string fromName);

            var passwordRecoveryEmail = emailSettings.GetSection("PasswordRecoveryEmail");
            var subject = passwordRecoveryEmail.GetSection("Subject").Value;

            var filePath = Path.Combine("Templates", "PasswordRecoveryEmail.html");

            _templateBuilder.BuildDefaultEmail(toName, toEmail, FromEmail, fromName, subject, filePath,
                out MimeMessage message, out BodyBuilder builder, out string urlBase);

            _templateBuilder.ReplaceTags(ref builder, "#{newPasswordLink}#",
                string.Concat(urlBase, "/Email/NewPassword?ChangePasswordToken=", token));

            _ = SendMessage(host, port, userName, password, message, builder);
        }

        private static MailKit.Net.Smtp.SmtpClient SendMessage(
            string host, int port, string userName, string password, MimeMessage message, BodyBuilder builder)
        {
            message.Body = builder.ToMessageBody();
            var client = new MailKit.Net.Smtp.SmtpClient();
            client.Connect(host, port, SecureSocketOptions.None);
            client.Authenticate(userName, password);
            client.Send(message);
            client.Disconnect(true);
            return client;
        }

        private void ConfigureEmailSettings(out IConfigurationSection emailSettings, out string host, out int port,
            out string userName, out string password, out string FromEmail, out string fromName)
        {
            emailSettings = _configuration.GetSection("EmailSettings");
            host = emailSettings.GetSection("Host").Value;
            port = int.Parse(emailSettings.GetSection("Port").Value);
            userName = emailSettings.GetSection("Username").Value;
            password = Environment.GetEnvironmentVariable("notguiriEmailPassword", EnvironmentVariableTarget.User) ?? emailSettings.GetSection("Password").Value;
            var defaultEmail = emailSettings.GetSection("DefaultEmail");
            FromEmail = defaultEmail.GetSection("FromEmail").Value;
            fromName = defaultEmail.GetSection("FromEmail").Value;
        }
    }
}
