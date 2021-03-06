using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.Common.Services.Token;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace NG.Auth.Business.Impl
{
    public class EmailService : IEmailService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly ITokenService _tokenService;
        private readonly ITokenHandler _tokenHandler;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<EmailService> _logger;
        private readonly Dictionary<BusinessErrorType, BusinessErrorObject> _errors;

        public EmailService(
            IAuthUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuthorizationProvider authorizationProvider,
            ITokenService tokenService,
            ITokenHandler tokenHandler,
            IEmailSender emailSender,
            ILogger<EmailService> logger,
            IOptions<Dictionary<BusinessErrorType, BusinessErrorObject>> errors)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _authorizationProvider = authorizationProvider;
            _tokenService = tokenService;
            _tokenHandler = tokenHandler;
            _emailSender = emailSender;
            _logger = logger;
            _errors = errors.Value;
        }

        public ConfirmationEmailStatus ResendConfirmationEmail(string AccessToken)
        {
            var tokenClaims = _tokenService.DecodeToken(AccessToken).Claims;
            var userId = Guid.Parse(tokenClaims.First(c => string.Equals(c.Type, ClaimTypes.NameIdentifier)).Value);
            var standardUser = _unitOfWork.StandardUser.Get(userId);

            if (standardUser == null)
            {
                return ConfirmationEmailStatus.Error;
            }

            if (standardUser.EmailConfirmed)
            {
                return ConfirmationEmailStatus.EmailAlreadyConfirmed;
            }

            SendConfirmationEmailToUser(standardUser);

            return ConfirmationEmailStatus.EmailSent;
        }

        private AuthenticationResponse SendConfirmationEmailToUser(StandardUser standardUser)
        {
            var authenticationResponse = GetAuthenticationResponse(standardUser);
            var firstName = standardUser.User.Name.Split(" ");
            _emailSender.SendConfirmationEmail(firstName[0], standardUser.User.Email, authenticationResponse.RefreshToken, authenticationResponse.AccessToken);
            return authenticationResponse;
        }

        public bool SendPasswordRecoveryEmail(string email)
        {
            var user = _unitOfWork.User.GetByEmail(email);

            if (user == null)
                return false;

            var cacheToken = _tokenHandler.GenerateChangePasswordToken(user.Email);

            var firstName = user.Name.Split(" ");
            _emailSender.SendPasswordRecoveryEmail(firstName[0], user.Email, cacheToken);

            return true;
        }

        private AuthenticationResponse GetAuthenticationResponse(StandardUser standardUser)
        {
            AuthorizedUser authUser = new AuthorizedUser(
                standardUser.UserId, standardUser.User.Email, standardUser.User.Role.ToString(), standardUser.EmailConfirmed);

            return new AuthenticationResponse(
                _authorizationProvider.GetToken(authUser),
                _tokenHandler.GenerateRefreshToken(authUser));
        }

        public User GetUser(string changePasswordToken)
        {
            var email = _tokenHandler.GetEmailFromCache(changePasswordToken);

            return _unitOfWork.User.GetByEmail(email);
        }
    }
}
