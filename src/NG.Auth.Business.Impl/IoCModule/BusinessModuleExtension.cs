using Microsoft.Extensions.DependencyInjection;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Business.Impl.InternalServices;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using NG.DBManager.Infrastructure.Impl.EF.IoCModule;
using NG.DBManager.Infrastructure.Impl.EF.UnitsOfWork;
using System;

namespace NG.Auth.Business.Impl.IoCModule
{
    public static class BusinessModuleExtension
    {
        public static IServiceCollection AddBusinessServices(
           this IServiceCollection services)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            services.AddInfrastructureServices()
                    .AddScoped<IPasswordHasher, PasswordHasher>()
                    .AddScoped<IAuthorizationProvider, AuthorizationProvider>()
                    .AddScoped<IAuthUnitOfWork, AuthUnitOfWork>()
                    .AddScoped<IUserService, UserService>();

            return services;
        }
    }
}
