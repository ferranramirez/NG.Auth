using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using NG.Auth.Test.E2ETest.Fixture;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Xunit;

namespace NG.Auth.Test.E2ETest
{
    public class UserControllerTests
    {
        public HttpUtilities _httpUtilities;
        public IAuthorizationProvider _authorizationProvider;

        public UserControllerTests(HttpUtilities httpUtilities, IoCModule ioCModule)
        {
            _httpUtilities = httpUtilities;

            var Configuration = _httpUtilities.ServiceProvider.GetService<IConfiguration>();
            _authorizationProvider = ioCModule.BuildServiceProvider(Configuration).GetService<IAuthorizationProvider>();
        }

        [Fact]
        public async Task GetRequestToAuthorizedUser_ShouldReturnUserAsJson()
        {
            // Arrange
            var client = _httpUtilities.HttpClient;

            AuthorizedUser authUser = new AuthorizedUser("ferran@notguiri.com", "Admin");

            var token = _authorizationProvider.GetToken(authUser);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", token);

            // Act
            var httpResponse = await client.GetAsync("/User");

            // Assert
            httpResponse.EnsureSuccessStatusCode();

            string response = await httpResponse.Content.ReadAsStringAsync();
            Assert.NotNull(response);
            User model = JsonConvert.DeserializeObject<User>(response);
            Assert.NotNull(model);
        }
    }
}
