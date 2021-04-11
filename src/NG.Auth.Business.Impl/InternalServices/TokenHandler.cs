using Microsoft.Extensions.Caching.Distributed;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Common.Services.AuthorizationProvider;
using NG.Common.Services.Token;
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
        private readonly ITokenService _tokenService;

        public TokenHandler(
            IAuthUnitOfWork unitOfWork,
            IDistributedCache distributedCache,
            IAuthorizationProvider authorizationProvider,
            ITokenService tokenService)
        {
            _unitOfWork = unitOfWork;
            _distributedCache = distributedCache;
            _authorizationProvider = authorizationProvider;
            _tokenService = tokenService;
        }

        public User GetUser(AuthenticationRequest credentials)
        {
            if (string.IsNullOrEmpty(credentials.PhoneNumber))
            {
                return _unitOfWork.User
                    .GetByEmail(credentials.EmailAddress.ToLower());
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

            var refreshToken = Convert.ToBase64String(randomBytes).Replace('+', '.');

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

        public string GenerateChangePasswordToken(string email)
        {
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            var randomBytes = new byte[64];
            rngCryptoServiceProvider.GetBytes(randomBytes);

            var changePasswordToken = Convert.ToBase64String(randomBytes).Replace('+', '.');

            SaveStringTokenInCache(changePasswordToken, email);

            return changePasswordToken;
        }

        public void SaveStringTokenInCache(string refreshToken, string email)
        {
            DistributedCacheEntryOptions cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = new TimeSpan(0, 2, 0, 0)
            };
            _distributedCache.SetString(refreshToken, JsonSerializer.Serialize(email), cacheOptions);
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

        public string GetEmailFromCache(string changePasswordToken)
        {
            var emailCache = _distributedCache.GetString(changePasswordToken);
            if (emailCache == null) return null;

            return JsonSerializer.Deserialize<string>(emailCache);
        }

        public AuthorizedUser GetAuthorizedUserFromCache(string confirmationToken)
        {
            var authorizedUser = _distributedCache.GetString(confirmationToken);
            if (authorizedUser == null) return null;

            return JsonSerializer.Deserialize<AuthorizedUser>(authorizedUser);
        }

        public bool IsEmailConfirmed(string accessToken)
        {
            var authorizationHeader = string.Concat("bearer ", accessToken);

            var tokenClaims = _tokenService.GetClaims(authorizationHeader);

            var emailConfirmed = tokenClaims.First(c => string.Equals(c.Type, "EmailConfirmed")).Value == "True";

            return emailConfirmed;
        }
    }
}
