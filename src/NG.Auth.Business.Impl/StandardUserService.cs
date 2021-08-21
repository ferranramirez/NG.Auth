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
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public class StandardUserService : IStandardUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IEmailSender _emailSender;
        private readonly ITokenService _tokenService;
        private readonly ITokenHandler _tokenHandler;
        private readonly IUserService _userService;
        private readonly ILogger<StandardUserService> _logger;
        private readonly Dictionary<BusinessErrorType, BusinessErrorObject> _errors;

        public StandardUserService(
            IAuthUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuthorizationProvider authorizationProvider,
            IEmailSender emailSender,
            ITokenService tokenService,
            ITokenHandler tokenHandler,
            IUserService userService,
            ILogger<StandardUserService> logger,
            IOptions<Dictionary<BusinessErrorType, BusinessErrorObject>> errors)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _authorizationProvider = authorizationProvider;
            _emailSender = emailSender;
            _tokenService = tokenService;
            _tokenHandler = tokenHandler;
            _userService = userService;
            _logger = logger;
            _errors = errors.Value;
        }

        public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            var user = _unitOfWork.User.GetByEmail(registerRequest.Email);

            if (user == null)
            {
                user = new User()
                {
                    Name = registerRequest.Name,
                    Birthdate = registerRequest.Birthdate,
                    PhoneNumber = registerRequest.PhoneNumber,
                    Email = registerRequest.Email.ToLower(),
                    Role = registerRequest.IsCommerce ? Role.Commerce : Role.Basic,
                    Image = null,
                };
            }

            StandardUser standardUser = new StandardUser()
            {
                User = user,
                UserId = user.Id,
                Password = registerRequest.Password,
            };

            _unitOfWork.StandardUser.Add(standardUser);

            await _unitOfWork.CommitAsync();

            return SendEmailToUser(standardUser);
        }

        public AuthenticationResponse Authenticate(StandardAuthenticationRequest credentials)
        {
            StandardUser standardUser = _tokenHandler.GetUser(credentials);

            if (standardUser == null)
            {
                var error = _errors[BusinessErrorType.UserNotFound];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            if (!standardUser.EmailConfirmed)
            {
                var error = _errors[BusinessErrorType.EmailNotConfirmed];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(standardUser.Password, credentials.Password);

            if (!Verified)
            {
                var error = _errors[BusinessErrorType.WrongPassword];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            return _userService.GetAuthenticationResponse(GetAuthUser(standardUser));
        }

        public string GetToken(StandardAuthenticationRequest credentials)
        {
            StandardUser standardUser = _tokenHandler.GetUser(credentials);

            if (standardUser == null)
            {
                var error = _errors[BusinessErrorType.UserNotFound];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(standardUser.Password, credentials.Password);

            if (!Verified)
            {
                var error = _errors[BusinessErrorType.WrongPassword];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }
            
            return _userService.GetAccessToken(GetAuthUser(standardUser));
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
                _unitOfWork.StandardUser.GetByEmail(authorizedUser.Email).EmailConfirmed == true)
            {
                return ConfirmationEmailStatus.EmailAlreadyConfirmed;
            }

            _unitOfWork.StandardUser.ConfirmEmail(authorizedUser.UserId);

            if (await _unitOfWork.CommitAsync() == 1)
            {
                return ConfirmationEmailStatus.EmailConfirmed;
            }

            return ConfirmationEmailStatus.TokenExpired;
        }

        public async Task<bool> UpdatePassword(string token, string password)
        {
            string email = _tokenHandler.GetEmailFromCache(token);

            if (email == null)
                return false;

            var standardUser = _unitOfWork.StandardUser.GetByEmail(email);
            standardUser.Password = password;
            _unitOfWork.StandardUser.Edit(standardUser);
            var rows = await _unitOfWork.CommitAsync();

            return rows > 0;
        }

        private AuthenticationResponse SendEmailToUser(StandardUser standardUser)
        {
            var authenticationResponse = _userService.GetAuthenticationResponse(GetAuthUser(standardUser));
            var firstName = standardUser.User.Name.Split(" ");
            _emailSender.SendConfirmationEmail(firstName[0], standardUser.User.Email, authenticationResponse.RefreshToken, authenticationResponse.AccessToken);
            return authenticationResponse;
        }
        private static AuthorizedUser GetAuthUser(StandardUser standardUser)
        {
            return new AuthorizedUser(standardUser.UserId, standardUser.User.Email, standardUser.User.Role.ToString(), standardUser.EmailConfirmed);
        }
    }
}
