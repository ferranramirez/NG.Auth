using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.Common.Services.Token;
using NG.DBManager.Infrastructure.Contracts.Contexts;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using NG.DBManager.Infrastructure.Impl.EF.UnitsOfWork;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public class UserService : IUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly ITokenService _tokenService;
        private readonly ITokenHandler _tokenHandler;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<UserService> _logger;
        private readonly Dictionary<BusinessErrorType, BusinessErrorObject> _errors;

        public UserService(
            IAuthUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuthorizationProvider authorizationProvider,
            ITokenService tokenService,
            ITokenHandler tokenHandler,
            IEmailSender emailSender,
            ILogger<UserService> logger,
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

        public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Name = registerRequest.Name,
                Birthdate = registerRequest.Birthdate,
                PhoneNumber = registerRequest.PhoneNumber,
                Email = registerRequest.Email.ToLower(),
                Password = registerRequest.Password,
                Role = registerRequest.IsCommerce ? Role.Commerce : Role.Basic,
                Image = null,
            };

            _unitOfWork.User.Add(user);
            await _unitOfWork.CommitAsync();

            return SendEmailToUser(user);
        }

        public AuthenticationResponse Authenticate(AuthenticationRequest credentials)
        {
            User user = _tokenHandler.GetUser(credentials);

            if (user == null)
            {
                var error = _errors[BusinessErrorType.UserNotFound];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            if (!user.EmailConfirmed)
            {
                var error = _errors[BusinessErrorType.EmailNotConfirmed];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(user.Password, credentials.Password);

            if (!Verified)
            {
                var error = _errors[BusinessErrorType.WrongPassword];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            return GetAuthenticationResponse(user);
        }

        public string GetToken(AuthenticationRequest credentials)
        {
            User user = _tokenHandler.GetUser(credentials);

            if (user == null)
            {
                var error = _errors[BusinessErrorType.UserNotFound];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(user.Password, credentials.Password);

            if (!Verified)
            {
                var error = _errors[BusinessErrorType.WrongPassword];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            return GetAccessToken(user);
        }

        public AuthenticationResponse RefreshToken(string refreshToken)
        {
            var authenticationResponse = _tokenHandler.Authenticate(refreshToken);

            if (authenticationResponse == null)
            {
                var error = _errors[BusinessErrorType.WrongToken];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            var emailConfirmed = _tokenHandler.IsEmailConfirmed(authenticationResponse.AccessToken);

            if (!emailConfirmed)
            {
                var error = _errors[BusinessErrorType.EmailNotConfirmed];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            return authenticationResponse;
        }

        public async Task<ConfirmationEmailStatus> ConfirmEmail(string confirmationToken /*is a 'refreshToken'*/, string accessToken)
        {
            AuthorizedUser authorizedUser = _tokenHandler.GetAuthorizedUserFromCache(confirmationToken);

            if (authorizedUser == null)
            {
                return ConfirmationEmailStatus.TokenExpired;
            }

            if (authorizedUser.EmailConfirmed ||
                _unitOfWork.User.GetByEmail(authorizedUser.Email).EmailConfirmed == true)
            {
                return ConfirmationEmailStatus.EmailAlreadyConfirmed;
            }

            _unitOfWork.User.ConfirmEmail(authorizedUser.UserId);

            if (await _unitOfWork.CommitAsync() == 1)
            {
                return ConfirmationEmailStatus.EmailConfirmed;
            }

            return ConfirmationEmailStatus.TokenExpired;
        }
        private AuthenticationResponse SendEmailToUser(User user)
        {
            var authenticationResponse = GetAuthenticationResponse(user);
            var firstName = user.Name.Split(" ");
            _emailSender.SendConfirmationEmail(firstName[0], user.Email, authenticationResponse.RefreshToken, authenticationResponse.AccessToken);
            return authenticationResponse;
        }

        private AuthenticationResponse GetAuthenticationResponse(User user)
        {
            AuthorizedUser authUser = new AuthorizedUser(user.Id, user.Email, user.Role.ToString(), user.EmailConfirmed);

            return new AuthenticationResponse(
                _authorizationProvider.GetToken(authUser),
                _tokenHandler.GenerateRefreshToken(authUser));
        }

        private string GetAccessToken(User user)
        {
            AuthorizedUser authUser = new AuthorizedUser(user.Id, user.Email, user.Role.ToString(), user.EmailConfirmed);
            return _authorizationProvider.GetToken(authUser);
        }

        public async Task<bool> UpdatePassword(string token, string password)
        {
            string email = _tokenHandler.GetEmailFromCache(token);

            if (email == null)
                return false;

            var user = _unitOfWork.User.GetByEmail(email);
            user.Password = password;
            _unitOfWork.User.Edit(user);
            var rows = await _unitOfWork.CommitAsync();

            if (rows > 0)
                return true;

            return false;
        }
    }
}
