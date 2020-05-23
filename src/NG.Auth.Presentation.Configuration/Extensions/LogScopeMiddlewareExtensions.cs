using Microsoft.AspNetCore.Builder;
using NG.Auth.Presentation.Configuration.Middleware;

namespace NG.Auth.Presentation.Configuration.Extensions
{
    public static class LogScopeMiddlewareExtensions
    {
        public static IApplicationBuilder UseLogScopeMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LogScopeMiddleware>();
        }
    }
}
