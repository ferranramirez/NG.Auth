using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public class UserService : IUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly ILogger<UserService> _logger;
        private readonly Dictionary<BusinessErrorType, BusinessErrorObject> _errors;

        public UserService(
            IAuthUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuthorizationProvider authorizationProvider,
            ILogger<UserService> logger,
            IOptions<Dictionary<BusinessErrorType, BusinessErrorObject>> errors)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _authorizationProvider = authorizationProvider;
            _logger = logger;
            _errors = errors.Value;
        }

        public async Task<bool> RegisterAsync(User user)
        {
            user.Password = _passwordHasher.Hash(user.Password);
            _unitOfWork.User.Add(user);
            return await _unitOfWork.CommitAsync() == 1;
        }

        public string Authenticate(Credentials credentials)
        {
            var user = _unitOfWork.User
                .GetByEmail(credentials.EmailAddress);

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

            AuthorizedUser authUser = new AuthorizedUser(user.Email, user.Role.ToString());

            return _authorizationProvider.GetToken(authUser);
        }
    }
}
