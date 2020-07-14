using Microsoft.Extensions.Caching.Distributed;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace NG.Auth.Business.Impl.InternalServices
{
    public sealed class TokenHandler : ITokenHandler
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IDistributedCache _distributedCache;
        private readonly IAuthorizationProvider _authorizationProvider;

        public TokenHandler(
            IAuthUnitOfWork unitOfWork,
            IDistributedCache distributedCache,
            IAuthorizationProvider authorizationProvider)
        {
            _unitOfWork = unitOfWork;
            _distributedCache = distributedCache;
            _authorizationProvider = authorizationProvider;
        }

        public User GetUser(AuthenticationRequest credentials)
        {
            if (string.IsNullOrEmpty(credentials.PhoneNumber))
            {
                return _unitOfWork.User
                    .GetByEmail(credentials.EmailAddress);
            }
            else
            {
                return _unitOfWork.User
                   .Find(u => u.PhoneNumber == credentials.PhoneNumber)
                   .SingleOrDefault();
            }
        }

        public string GenerateRefreshToken(AuthorizedUser authorizedUser)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);

            var refreshToken = Convert.ToBase64String(randomBytes);

            SaveRefreshTokenInCache(refreshToken, authorizedUser);

            return refreshToken;
        }

        public void SaveRefreshTokenInCache(string refreshToken, AuthorizedUser authorizedUser)
        {
            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(7, 0, 0, 0)
            };
            _distributedCache.SetString(refreshToken, JsonSerializer.Serialize(authorizedUser), cacheOptions);
        }

        public AuthenticationResponse Authenticate(string refreshToken)
        {
            var user = _distributedCache.GetString(refreshToken);

            if (user == null)
                return null;

            AuthorizedUser authorizedUser = JsonSerializer.Deserialize<AuthorizedUser>(user);

            return new AuthenticationResponse(
                    _authorizationProvider.GetToken(authorizedUser),
                    GenerateRefreshToken(authorizedUser));
        }
    }
}
