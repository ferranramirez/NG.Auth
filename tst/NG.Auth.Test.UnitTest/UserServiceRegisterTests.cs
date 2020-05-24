using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Business.Impl;
using NG.Auth.Test.UnitTest.Fixture;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Threading.Tasks;
using Xunit;

namespace NG.Auth.Test.UnitTest
{
    public class UserServiceRegisterTests : IClassFixture<IoCModule>
    {
        private readonly Mock<IAuthUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthorizationProvider> _authorizationProviderMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly NullLogger<UserService> _nullLogger;
        private readonly IUserService _userService;
        private readonly User user;
        private readonly string expected;
        private readonly string hashedPassword;

        public UserServiceRegisterTests()
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "basic@test.org",
                Password = "secret123",
            };

            hashedPassword = "8e84ab1bfd51f941d7cd5b5ef3857c9c6238edd6790adc50";

            _unitOfWorkMock = new Mock<IAuthUnitOfWork>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _authorizationProviderMock = new Mock<IAuthorizationProvider>();
            _nullLogger = new NullLogger<UserService>();

            _userService = new UserService(_unitOfWorkMock.Object, _passwordHasherMock.Object, _authorizationProviderMock.Object, _nullLogger);
        }

        [Fact]
        public async Task UserService_RegisterUser_ReturnsTrueAsync()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.User.Add(user));
            _unitOfWorkMock.Setup(uow => uow.CommitAsync()).Returns(Task.FromResult(1));
            _passwordHasherMock.Setup(pwdH => pwdH.Hash("secret123")).Returns(hashedPassword);

            // Act
            var actual = await _userService.RegisterAsync(user);

            // Assert
            Assert.True(actual);
        }
    }
}
