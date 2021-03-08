using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using NG.Common.Library.Filters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace NG.Auth.Presentation.WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class EmailController : Controller
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailController> _logger;
        private readonly IConfiguration _configuration;
        private readonly ITemplateBuilder _templateBuilder;

        public EmailController(
            IEmailService emailService,
            ILogger<EmailController> logger,
            IConfiguration configuration,
            ITemplateBuilder templateBuilder)
        {
            _emailService = emailService;
            _logger = logger;
            _configuration = configuration;
            _templateBuilder = templateBuilder;
        }

        /// <summary>
        /// Resend confirmation email
        /// </summary>
        /// <param name="AccessToken">The confirmation token to confirm the user's email.</param>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - Email successfully confirmed.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        [HttpGet("ResendConfirmation")]
        [ProducesResponseType(typeof(ApiError), 543)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AuthenticationResponse), (int)HttpStatusCode.OK)]
        public IActionResult ResendConfirmation([FromQuery] string AccessToken)
        {
            var emailStatus = _emailService.ResendConfirmationEmail(AccessToken);

            if (emailStatus == ConfirmationEmailStatus.EmailAlreadyConfirmed)
                return View("EmailAlreadyConfirmed");

            if (emailStatus == ConfirmationEmailStatus.Error)
                return View("Error");

            return View("EmailSent");
        }

        /// <summary>
        /// Send an email to recover password
        /// </summary>
        /// <param name="email">The email from the user who wants to change their password.</param>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - Recover password email successfully sent.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        [HttpGet("SendPasswordRecovery")]
        [ProducesResponseType(typeof(ApiError), 543)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AuthenticationResponse), (int)HttpStatusCode.OK)]
        public IActionResult SendPasswordRecovery([FromQuery] string email)
        {
            var emailStatus = _emailService.SendPasswordRecoveryEmail(email);
            return Ok(emailStatus);
        }

        /// <summary>
        /// Send an email to recover password
        /// </summary>
        /// <param name="ChangePasswordToken">The token to change the user's password.</param>
        /// <remarks>
        /// ## Response code meanings
        /// - 200 - Recover password email successfully sent.
        /// - 400 - The model is not properly built.
        /// - 500 - An internal server error. Something bad and unexpected happened.
        /// - 543 - A handled error. This error was expected, check the message.
        /// </remarks>
        [HttpGet("NewPassword")]
        [ProducesResponseType(typeof(ApiError), 543)]
        [ProducesResponseType(typeof(ApiError), (int)HttpStatusCode.InternalServerError)]
        [ProducesResponseType(typeof(Dictionary<string, string[]>), (int)HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(AuthenticationResponse), (int)HttpStatusCode.OK)]
        public IActionResult ChangePasswordView([FromQuery] string ChangePasswordToken)
        {
            // var user = _emailService.GetUser(ChangePasswordToken);

            var baseUrl = _configuration.GetSection("Urls").GetSection("Base").Value;
            string newPasswordLink = string.Concat(baseUrl, "/User/ChangePassword");
            string errorLink = string.Concat(baseUrl, "/Error");

            var newPasswordModel = new NewPasswordInfo(newPasswordLink, errorLink, ChangePasswordToken);

            return View("NewPassword", newPasswordModel);
        }
    }

    public class NewPasswordInfo
    {
        public NewPasswordInfo(string ResetURL, string ErrorURL, string ChangePasswordToken)
        {
            this.ResetURL = ResetURL;
            this.ErrorURL = ErrorURL;
            this.ChangePasswordToken = ChangePasswordToken;
        }

        public string ResetURL { get; set; }
        public string ErrorURL { get; set; }
        public string ChangePasswordToken { get; set; }
    }
}