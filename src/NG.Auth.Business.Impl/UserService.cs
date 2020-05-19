using Microsoft.Extensions.Logging;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Linq;

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

        public bool Register(User user)
        {
            user.Password = _passwordHasher.Hash(user.Password);
            _unitOfWork.User.Add(user);
            return _unitOfWork.Commit() == 1;
        }

        public string Authenticate(Credentials credentials)
        {
            var user = _unitOfWork.User
                .Find(u => u.Email == credentials.EmailAddress)
                .SingleOrDefault();

            if (user == null)
            {
                _logger.LogError("Not found user with such email address {0}", credentials.EmailAddress);
                throw new Exception("Not found user with such email address ");
            }

            var (Verified, NeedsUpgrade) = _passwordHasher.Check(user.Password, credentials.Password);

            if (!Verified)
            {
                _logger.LogError("Wrong password ({0}), for the user {1}", credentials.Password, credentials.EmailAddress);
                throw new Exception("Wrong password: The given password does not match the stored password in the database");
            }

            _logger.LogInformation("User {0} {1} successfully logged in", user.Name, user.Surname);

            return _authorizationProvider.GetToken(user);
        }
    }

}
