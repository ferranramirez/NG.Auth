using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace NG.Auth.Presentation.WebAPI.Middleware
{
    public class ErrorsCheckMiddleware
    {
        private readonly RequestDelegate _next;

        public ErrorsCheckMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context, IConfiguration configuration)
        {
            if (context.Request.Path.Value.Contains("/errors"))
            {
                var errors = configuration.GetSection("Errors");

                var report = errors.GetChildren().Select(x => x.Key);

                await context.Response.WriteAsync(string.Join("\n", report));
            }
            else
            {
                await _next(context);
            }
        }
    }
}
