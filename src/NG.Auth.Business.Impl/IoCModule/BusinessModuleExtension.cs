using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Business.Impl.InternalServices;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.Common.Services.Token;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using NG.DBManager.Infrastructure.Impl.EF.Extensions;
using NG.DBManager.Infrastructure.Impl.EF.UnitsOfWork;
using System;
using System.Collections.Generic;

namespace NG.Auth.Business.Impl.IoCModule
{
    public static class BusinessModuleExtension
    {
        public static IServiceCollection AddBusinessServices(
           this IServiceCollection services,
           IConfiguration configuration)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddInfrastructureServices()
                    .AddSingleton<IPasswordHasher, PasswordHasher>()
                    .AddSingleton<IEmailSender, EmailSender>()
                    .AddSingleton<ITokenService, TokenService>()
                    .AddSingleton<ITemplateBuilder, TemplateBuilder>()
                    .AddScoped<IAuthorizationProvider, AuthorizationProvider>()
                    .AddScoped<ITokenHandler, TokenHandler>()
                    .AddScoped<IUserService, UserService>()
                    .AddScoped<IAuthUnitOfWork, AuthUnitOfWork>()
                    .AddScoped<IEmailService, EmailService>()
                    .AddScoped<IStandardUserService, StandardUserService>()
                    .AddScoped<ISocialUserService, SocialUserService>()
                    .Configure<Dictionary<BusinessErrorType, BusinessErrorObject>>(x => configuration.GetSection("Errors").Bind(x));

            return services;
        }
    }
}
