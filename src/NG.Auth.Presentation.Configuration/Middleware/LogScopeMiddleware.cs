using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NG.Auth.Presentation.Configuration.Middleware
{
    public class LogScopeMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LogScopeMiddleware> _logger;

        public LogScopeMiddleware(
            RequestDelegate next,
            ILogger<LogScopeMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var tokenClaims = GetTokenClaims(context.Request.Headers["Authorization"]);
            string email = tokenClaims.Any() ?
                    tokenClaims.First(c => string.Equals(c.Type, ClaimTypes.Email)).Value : "anonymous";
            string role = tokenClaims.Any() ?
                    tokenClaims.First(c => string.Equals(c.Type, ClaimTypes.Role)).Value : "none";

            using (_logger.BeginScope(email))
            using (_logger.BeginScope(role))
            {
                await _next(context);
            }
        }

        private IEnumerable<Claim> GetTokenClaims(StringValues authorizationHeader)
        {
            if (authorizationHeader.Any())
            {
                var token = AuthenticationHeaderValue.Parse(authorizationHeader);

                var handler = new JwtSecurityTokenHandler();
                var tokenS = handler.ReadToken(token.Parameter) as JwtSecurityToken;

                return tokenS.Claims;
            }
            return new List<Claim>();
        }
    }
}
