using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Business.Impl;
using NG.Auth.Domain;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Collections.Generic;
using Xunit;

namespace NG.Auth.Test.UnitTest
{
    public class UserServiceAuthenticateTests
    {
        private readonly Mock<IAuthUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthorizationProvider> _authorizationProviderMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly NullLogger<UserService> _nullLogger;
        private readonly IUserService _userService;
        private readonly User user;
        private readonly AuthorizedUser authUser;
        private readonly AuthenticationResponse expected;

        public UserServiceAuthenticateTests()
        {
            var userId = Guid.NewGuid();

            authUser = new AuthorizedUser(userId, "basic@test.org", "Basic");

            expected = new AuthenticationResponse(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJiYXNpY0B0ZXN0Lm9yZyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJCYXNpYyJ9.JFy76_gBh - i3NFBa2xO_k - h3k8ygqFlv1Qos94xvKvM",
                "refreshToken");

            user = new User
            {
                Id = userId,
                Name = "Test",
                Email = "basic@test.org",
                Password = "secret123",
                Role = Role.Basic
            };

            _unitOfWorkMock = new Mock<IAuthUnitOfWork>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _authorizationProviderMock = new Mock<IAuthorizationProvider>();
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _nullLogger = new NullLogger<UserService>();

            var errorsDictionary = new Dictionary<BusinessErrorType, BusinessErrorObject>
            {
                { BusinessErrorType.UserNotFound, new BusinessErrorObject() { ErrorCode = 101, Message = "Error test" } },
                { BusinessErrorType.WrongPassword, new BusinessErrorObject() { ErrorCode = 102, Message = "Error test" } },
                { BusinessErrorType.WrongToken, new BusinessErrorObject() { ErrorCode = 103, Message = "Error test" } }
            };
            var _options = Options.Create(errorsDictionary);

            _userService = new UserService(_unitOfWorkMock.Object, _passwordHasherMock.Object,
                _authorizationProviderMock.Object, _tokenHandlerMock.Object, _nullLogger, _options);
        }

        [Fact]
        public void UserService_AuthenticateUser_ReturnsRightToken()
        {
            // Arrange
            AuthenticationRequest credentials = new AuthenticationRequest()
            {
                EmailAddress = "basic@test.org",
                Password = "secret123"
            };
            _tokenHandlerMock.Setup(tknH => tknH.GetUser(credentials)).Returns(user);
            _passwordHasherMock.Setup(pwdH => pwdH.Check("secret123", "secret123")).Returns((true, false));
            _authorizationProviderMock.Setup(authP => authP.GetToken(authUser)).Returns(expected.AccessToken);
            _tokenHandlerMock.Setup(tknH => tknH.GenerateRefreshToken(authUser)).Returns(expected.RefreshToken);

            // Act
            var actual = _userService.Authenticate(credentials);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UserService_AuthenticateUserwithWrongEmail_ThrowsCustomException()
        {
            // Arrange
            AuthenticationRequest credentials = new AuthenticationRequest()
            {
                EmailAddress = "WRONG_basic@test.org",
                Password = "secret123"
            };
            _tokenHandlerMock.Setup(tknH => tknH.GetUser(credentials)).Returns((User)null);
            //_errorsMock.Setup(errors => errors.Value[0].ErrorCode).Returns(101);

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
            AuthenticationRequest credentials = new AuthenticationRequest()
            {
                EmailAddress = "basic@test.org",
                Password = "WRONG_secret123"
            };
            _tokenHandlerMock.Setup(tknH => tknH.GetUser(credentials)).Returns(user);

            // Act
            Action action = () => _userService.Authenticate(credentials);

            // Assert
            var exception = Assert.Throws<NotGuiriBusinessException>(action);
            Assert.Equal(102, exception.ErrorCode);
        }
    }
}
