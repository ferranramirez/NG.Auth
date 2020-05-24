using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Impl;
using NG.Common.Services.AuthorizationProvider;

namespace NG.Auth.Test.UnitTest.Fixture
{
    public class IoCModule
    {
        public IoCModule()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection
                .AddTransient<IConfiguration>(sp =>
                {
                    IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
                    configurationBuilder.AddJsonFile("appsettings.Development.json");
                    return configurationBuilder.Build();
                })
                .AddTransient<IAuthorizationProvider, AuthorizationProvider>()
                .AddTransient<IUserService, UserService>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
