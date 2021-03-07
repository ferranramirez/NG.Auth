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
    public class TemplateBuilder : ITemplateBuilder
    {
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public TemplateBuilder(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public void ReplaceTags(ref BodyBuilder builder, string tag, string replacementText)
        {
            builder.HtmlBody = builder.HtmlBody.Replace(tag, replacementText);
        }

        public void BuildDefaultEmail(string toName, string toEmail, string FromEmail, string fromName, string subject,
            string filePath, out MimeMessage message, out BodyBuilder builder, out string urlBase)
        {
            message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            builder = OpenFile(filePath);

            urlBase = _configuration.GetSection("Urls").GetSection("Base").Value;
            builder.HtmlBody = builder.HtmlBody.Replace("#{userName}#", toName);
            builder.HtmlBody = builder.HtmlBody.Replace("#{userEmail}#", toEmail);
        }

        public BodyBuilder OpenFile(string filePath)
        {
            var fileFullPath = Path.Combine(_environment.ContentRootPath, filePath);

            BodyBuilder builder = new BodyBuilder();
            using (StreamReader SourceReader = File.OpenText(fileFullPath))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }

            return builder;
        }
    }
}
