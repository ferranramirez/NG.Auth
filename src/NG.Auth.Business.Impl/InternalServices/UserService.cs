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
    public class UserService : IUserService
    {
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly ITokenHandler _tokenHandler;
        private readonly IAuthUnitOfWork _unitOfWork;

        public UserService(
            IAuthorizationProvider authorizationProvider,
            ITokenHandler tokenHandler,
            IAuthUnitOfWork unitOfWork)
        {
            _authorizationProvider = authorizationProvider;
            _tokenHandler = tokenHandler;
            _unitOfWork = unitOfWork;
        }
        public AuthenticationResponse GetAuthenticationResponse(AuthorizedUser authUser)
        {
            return new AuthenticationResponse(
                _authorizationProvider.GetToken(authUser),
                _tokenHandler.GenerateRefreshToken(authUser));
        }

        public string GetAccessToken(AuthorizedUser authUser)
        {
            return _authorizationProvider.GetToken(authUser);
        }

        public User GetExistingUser(CommonRegisterFields registerRequest)
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

            return user;
        }
    }
}
