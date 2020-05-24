using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Business.Impl;
using NG.Auth.Domain;
using NG.Auth.Test.UnitTest.Fixture;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using Xunit;

namespace NG.Auth.Test.UnitTest
{
    public class UserServiceAuthenticateTests : IClassFixture<IoCModule>
    {
        private readonly Mock<IAuthUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthorizationProvider> _authorizationProviderMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly NullLogger<UserService> _nullLogger;
        private readonly IUserService _userService;
        private readonly User user;
        private readonly AuthorizedUser authUser;
        private readonly string expected;

        public UserServiceAuthenticateTests()
        {
            user = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "basic@test.org",
                Password = "secret123",
                Role = Role.Basic
            };

            authUser = new AuthorizedUser("basic@test.org", "Basic");

            expected = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJiYXNpY0B0ZXN0Lm9yZyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJCYXNpYyJ9.JFy76_gBh - i3NFBa2xO_k - h3k8ygqFlv1Qos94xvKvM";

            _unitOfWorkMock = new Mock<IAuthUnitOfWork>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _authorizationProviderMock = new Mock<IAuthorizationProvider>();
            _nullLogger = new NullLogger<UserService>();

            _userService = new UserService(_unitOfWorkMock.Object, _passwordHasherMock.Object, _authorizationProviderMock.Object, _nullLogger);
        }

        [Fact]
        public void UserService_AuthenticateUser_ReturnsRightToken()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.User.GetByEmail("basic@test.org")).Returns(user);
            _passwordHasherMock.Setup(pwdH => pwdH.Check("secret123", "secret123")).Returns((true, false));
            _authorizationProviderMock.Setup(authP => authP.GetToken(authUser)).Returns(expected);

            Credentials credentials = new Credentials()
            {
                EmailAddress = "basic@test.org",
                Password = "secret123"
            };

            // Act
            var actual = _userService.Authenticate(credentials);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UserService_AuthenticateUserwithWrongEmail_ThrowsCustomException()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.User.GetByEmail("WRONG_basic@test.org")).Returns((User)null);

            Credentials credentials = new Credentials()
            {
                EmailAddress = "WRONG_basic@test.org",
                Password = "secret123"
            };

            // Act
            Action action = () => _userService.Authenticate(credentials);

            // Assert
            var exception = Assert.Throws<NotGuiriBusinessException>(action);
            Assert.Equal(101, exception.ErrorCode);
        }

        [Fact]
        public void UserService_AuthenticateUserwithWrongPassword_ThrowsCustomException()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.User.GetByEmail("basic@test.org")).Returns(user);

            Credentials credentials = new Credentials()
            {
                EmailAddress = "basic@test.org",
                Password = "WRONG_secret123"
            };

            // Act
            Action action = () => _userService.Authenticate(credentials);

            // Assert
            var exception = Assert.Throws<NotGuiriBusinessException>(action);
            Assert.Equal(102, exception.ErrorCode);
        }
    }
}
