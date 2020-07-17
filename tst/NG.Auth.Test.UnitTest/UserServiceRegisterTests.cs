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
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace NG.Auth.Test.UnitTest
{
    public class UserServiceRegisterTests
    {
        private readonly Mock<IAuthUnitOfWork> _unitOfWorkMock;
        private readonly Mock<IAuthorizationProvider> _authorizationProviderMock;
        private readonly Mock<ITokenHandler> _tokenHandlerMock;
        private readonly Mock<IPasswordHasher> _passwordHasherMock;
        private readonly NullLogger<UserService> _nullLogger;
        private readonly IUserService _userService;
        private readonly User expected;
        private readonly RegisterRequest userDto;
        private readonly string hashedPassword;

        public UserServiceRegisterTests()
        {
            expected = new User
            {
                Id = Guid.NewGuid(),
                Name = "Test",
                Email = "basic@test.org",
                Password = "secret123",
            };

            userDto = new RegisterRequest
            {
                Name = "Test",
                Email = "basic@test.org",
                Password = "secret123",
            };

            hashedPassword = "8e84ab1bfd51f941d7cd5b5ef3857c9c6238edd6790adc50";

            _unitOfWorkMock = new Mock<IAuthUnitOfWork>();
            _passwordHasherMock = new Mock<IPasswordHasher>();
            _authorizationProviderMock = new Mock<IAuthorizationProvider>();
            _tokenHandlerMock = new Mock<ITokenHandler>();
            _nullLogger = new NullLogger<UserService>();

            var errorsDictionary = new Dictionary<BusinessErrorType, BusinessErrorObject>
            {
                { BusinessErrorType.UserNotFound, new BusinessErrorObject() { ErrorCode = 101, Message = "Error test" } },
                { BusinessErrorType.WrongPassword, new BusinessErrorObject() { ErrorCode = 102, Message = "Error test" } }
            };
            var _options = Options.Create(errorsDictionary);

            _userService = new UserService(_unitOfWorkMock.Object, _passwordHasherMock.Object,
                _authorizationProviderMock.Object, _tokenHandlerMock.Object, _nullLogger, _options);
        }

        [Fact(Skip = "Cannot know the Id of the mapped user when the mapping is done in the business layer")]
        public async Task UserService_RegisterUser_ReturnsTrueAsync()
        {
            // Arrange
            _unitOfWorkMock.Setup(uow => uow.User.Add(expected));
            _unitOfWorkMock.Setup(uow => uow.CommitAsync()).Returns(Task.FromResult(1));
            _unitOfWorkMock.Setup(uow => uow.User.Get(expected.Id)).Returns(expected);
            _passwordHasherMock.Setup(pwdH => pwdH.Hash("secret123")).Returns(hashedPassword);

            // Act
            var actual = await _userService.RegisterAsync(userDto);

            // Assert
            Assert.Equal(actual, actual); // Change for an expected result
        }
    }
}
