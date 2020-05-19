using Microsoft.Extensions.DependencyInjection;

namespace NG.Auth.Test.UnitTest.Fixture
{
    public class IoCModule
    {
        public IoCModule()
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            //HashingOptions hashingOptions = new HashingOptions();
            //hashingOptions.Iterations = 2000;
            //Action options = new Action
            //{
            //    Method = hashingOptions,
            //    options.
            //};

            //serviceCollection
            //    .Configure<HashingOptions>(options)
            //    .AddTransient<IPasswordHasher, PasswordHasher>();

            ServiceProvider = serviceCollection.BuildServiceProvider();
        }

        public ServiceProvider ServiceProvider { get; private set; }
    }
}
