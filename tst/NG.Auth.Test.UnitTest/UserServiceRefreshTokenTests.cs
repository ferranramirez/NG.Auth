using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Business.Impl;
using NG.Auth.Domain;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Collections.Generic;
using Xunit;

namespace NG.Auth.Test.UnitTest
{
    public class UserServiceRefreshTokenTests
    {
        private readonly Mock<IAuthUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthorizationProvider> _authorizationProviderMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly NullLogger<UserService> _nullLogger;
        private readonly IUserService _userService;
        private readonly AuthenticationResponse expected;

        public UserServiceRefreshTokenTests()
        {
            expected = new AuthenticationResponse(
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9lbWFpbGFkZHJlc3MiOiJiYXNpY0B0ZXN0Lm9yZyIsImh0dHA6Ly9zY2hlbWFzLnhtbHNvYXAub3JnL3dzLzIwMDUvMDUvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJCYXNpYyJ9.JFy76_gBh - i3NFBa2xO_k - h3k8ygqFlv1Qos94xvKvM",
                "refreshToken");

            _unitOfWorkMock = new Mock<IAuthUnitOfWork>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _authorizationProviderMock = new Mock<IAuthorizationProvider>();
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _nullLogger = new NullLogger<UserService>();

            var errorsDictionary = new Dictionary<BusinessErrorType, BusinessErrorObject>
            {
                { BusinessErrorType.WrongToken, new BusinessErrorObject() { ErrorCode = 103, Message = "Error test" } }
            };
            var _options = Options.Create(errorsDictionary);

            _userService = new UserService(_unitOfWorkMock.Object, _passwordHasherMock.Object,
                _authorizationProviderMock.Object, _tokenHandlerMock.Object, _nullLogger, _options);
        }

        [Fact]
        public void UserService_RefreshToken_ReturnsRightToken()
        {
            // Arrange
            _tokenHandlerMock.Setup(tknH => tknH.Authenticate("oldRefreshToken")).Returns(expected);

            // Act
            var actual = _userService.RefreshToken("oldRefreshToken");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void UserService_RefreshTokenWithWrongToken_ThrowsCustomException()
        {

            // Arrange
            _tokenHandlerMock.Setup(tknH => tknH.Authenticate("oldRefreshToken")).Returns(expected); // Not needed

            // Act
            Action action = () => _userService.RefreshToken("wrongRefreshToken");

            // Assert
            var exception = Assert.Throws<NotGuiriBusinessException>(action);
            Assert.Equal(103, exception.ErrorCode);
        }
    }
}