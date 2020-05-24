using Microsoft.Extensions.Logging;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Common.Library.Exceptions;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public class UserService : IUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly ILogger<UserService> _logger;

        public UserService(
            IAuthUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuthorizationProvider authorizationProvider,
            ILogger<UserService> logger)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _authorizationProvider = authorizationProvider;
            _logger = logger;
        }

        public async Task<bool> Register(User user)
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
                throw new NotGuiriBusinessException("User not found", 101);
            }

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(user.Password, credentials.Password);

            if (!Verified)
            {
                throw new NotGuiriBusinessException("Wrong password: The given password does not match the user's password", 102);
            }

            _logger.LogInformation("User {0} successfully logged in", user.Id);

            return _authorizationProvider.GetToken(user);
        }
    }
}
