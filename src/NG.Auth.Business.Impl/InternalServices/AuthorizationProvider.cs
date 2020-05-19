using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using NG.Auth.Business.Contract.InternalServices;
using NG.DBManager.Infrastructure.Contracts.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace NG.Auth.Business.Impl.InternalServices
{
    public class AuthorizationProvider : IAuthorizationProvider
    {
        private readonly string _authKey;

        public AuthorizationProvider(IConfiguration configuration)
        {
            _authKey = configuration.GetSection("Secrets").GetSection("AuthKey").Value;
        }

        public string GetToken(User user)
        {
            ClaimsIdentity identity = CreateIdentity(user);

            return GenerateToken(identity);
        }

        private ClaimsIdentity CreateIdentity(User user)
        {
            var identity = new ClaimsIdentity(IdentityConstants.ApplicationScheme);
            identity.AddClaim(new Claim(ClaimTypes.Email, user.Email));
            identity.AddClaim(new Claim(ClaimTypes.Role, user.Role.ToString()));
            return identity;
        }

        private string GenerateToken(ClaimsIdentity identity)
        {
            var key = System.Text.Encoding.ASCII.GetBytes(_authKey);

            var jwtSecurityToken = new JwtSecurityToken(
                notBefore: DateTime.UtcNow,
                claims: identity.Claims,
                expires: DateTime.UtcNow.AddDays(1),
                signingCredentials: new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
                );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }

}
