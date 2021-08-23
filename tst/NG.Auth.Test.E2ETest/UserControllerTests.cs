using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NG.Auth.Domain;
using NG.Auth.Test.E2ETest.Fixture;
using NG.Common.Services.AuthorizationProvider;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace NG.Auth.Test.E2ETest
{
    public class UserControllerTests : IClassFixture<HttpUtilities>, IClassFixture<IoCModule>
    {
        public HttpUtilities _httpUtilities;
        public IAuthorizationProvider _authorizationProvider;

        public UserControllerTests(HttpUtilities httpUtilities, IoCModule ioCModule)
        {
            _httpUtilities = httpUtilities;

            var Configuration = _httpUtilities.ServiceProvider.GetRequiredService<IConfiguration>();
            _authorizationProvider = ioCModule.BuildServiceProvider(Configuration).GetRequiredService<IAuthorizationProvider>();
        }

        [Fact]
        public async Task GetRequestToAuthorizedUser_ShouldReturnUserAsJson()
        {
            // Arrange
            StandardAuthenticationRequest credentials = new StandardAuthenticationRequest()
            {
                EmailAddress = "basic@test.org",
                Password = "basicPassword123"
            };

            var client = _httpUtilities.HttpClient;

            var credentialsJson = JsonConvert.SerializeObject(credentials);

            var stringContent = new StringContent(credentialsJson, Encoding.UTF8, "application/json");

            // Act
            var httpResponse = await client.PostAsync("/User/Authenticate", stringContent);

            // Assert
            httpResponse.EnsureSuccessStatusCode();

            string response = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(response);
        }
    }
}
