﻿using MailKit.Security;
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
        private readonly IWebHostEnvironment _environment;
        private readonly IConfiguration _configuration;

        public EmailSender(
            IWebHostEnvironment environment,
            IConfiguration configuration)
        {
            _environment = environment;
            _configuration = configuration;
        }

        public void SendEmailConfirmation(string toName, string toEmail, string token)
        {
            var emailSettings = _configuration.GetSection("EmailSettings");
            var host = emailSettings.GetSection("Host").Value;
            var port = int.Parse(emailSettings.GetSection("Port").Value);
            var userName = emailSettings.GetSection("Username").Value;
            var password = Environment.GetEnvironmentVariable("notguiriEmailPassword", EnvironmentVariableTarget.User) ?? emailSettings.GetSection("Password").Value;

            var confirmationEmail = emailSettings.GetSection("ConfirmationEmail");
            var FromEmail = confirmationEmail.GetSection("FromEmail").Value;
            var fromName = confirmationEmail.GetSection("FromEmail").Value;
            var subject = confirmationEmail.GetSection("Subject").Value;

            var filePath = Path.Combine(_environment.ContentRootPath, "Templates", "ConfirmationEmail.html");

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, FromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            using (StreamReader SourceReader = File.OpenText(filePath))
            {
                builder.HtmlBody = SourceReader.ReadToEnd();
            }

            var urlBase = _configuration.GetSection("Urls").GetSection("Base").Value;

            builder.HtmlBody = builder.HtmlBody.Replace("#{userName}#", toName);
            builder.HtmlBody = builder.HtmlBody.Replace("#{userEmail}#", toEmail);
            builder.HtmlBody = builder.HtmlBody.Replace("#{validationLink}#", string.Concat(urlBase, "/User?ConfirmationToken=", token));

            message.Body = builder.ToMessageBody();

            using var client = new MailKit.Net.Smtp.SmtpClient();
            client.Connect(host, port, SecureSocketOptions.None);
            client.Authenticate(userName, password);
            client.Send(message);
            client.Disconnect(true);
        }
    }
}