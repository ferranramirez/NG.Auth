using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public class UserService : IUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly ITokenHandler _tokenHandler;
        private readonly ILogger<UserService> _logger;
        private readonly Dictionary<BusinessErrorType, BusinessErrorObject> _errors;

        public UserService(
            IAuthUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuthorizationProvider authorizationProvider,
            ITokenHandler tokenHandler,
            ILogger<UserService> logger,
            IOptions<Dictionary<BusinessErrorType, BusinessErrorObject>> errors)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _authorizationProvider = authorizationProvider;
            _tokenHandler = tokenHandler;
            _logger = logger;
            _errors = errors.Value;
        }

        public async Task<AuthenticationResponse> RegisterAsync(RegisterRequest registerRequest)
        {
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Name = registerRequest.Name,
                Surname = registerRequest.Surname,
                Birthdate = registerRequest.Birthdate,
                PhoneNumber = registerRequest.PhoneNumber,
                Email = registerRequest.Email,
                Password = _passwordHasher.Hash(registerRequest.Password),
                Role = Role.Basic,
                Image = null,
            };

            _unitOfWork.User.Add(user);
            await _unitOfWork.CommitAsync();

            return GetAuthenticationResponse(user);
        }

        public AuthenticationResponse Authenticate(AuthenticationRequest credentials)
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

            return GetAuthenticationResponse(user);
        }

        private AuthenticationResponse GetAuthenticationResponse(User user)
        {
            AuthorizedUser authUser = new AuthorizedUser(user.Id, user.Email, user.Role.ToString());

            return new AuthenticationResponse(
                _authorizationProvider.GetToken(authUser),
                _tokenHandler.GenerateRefreshToken(authUser));
        }

        public AuthenticationResponse RefreshToken(string refreshToken)
        {
            var authenticationResponse = _tokenHandler.Authenticate(refreshToken);

            if (authenticationResponse == null)
            {
                var error = _errors[BusinessErrorType.WrongToken];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            return authenticationResponse;
        }
    }
}
