﻿using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NG.Auth.Business.Contract;
using NG.Auth.Business.Contract.InternalServices;
using NG.Auth.Domain;
using NG.Auth.Domain.ConfirmationEmailStatus;
using NG.Common.Library.Exceptions;
using NG.Common.Services.AuthorizationProvider;
using NG.Common.Services.Token;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using NG.DBManager.Infrastructure.Contracts.UnitsOfWork;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public class SocialUserService : ISocialUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IAuthorizationProvider _authorizationProvider;
        private readonly IEmailSender _emailSender;
        private readonly ITokenService _tokenService;
        private readonly ITokenHandler _tokenHandler;
        private readonly IUserService _userService;
        private readonly ILogger<SocialUserService> _logger;
        private readonly Dictionary<BusinessErrorType, BusinessErrorObject> _errors;

        public SocialUserService(
            IAuthUnitOfWork unitOfWork,
            IPasswordHasher passwordHasher,
            IAuthorizationProvider authorizationProvider,
            IEmailSender emailSender,
            ITokenService tokenService,
            ITokenHandler tokenHandler,
            IUserService userService,
            ILogger<SocialUserService> logger,
            IOptions<Dictionary<BusinessErrorType, BusinessErrorObject>> errors)
        {
            _unitOfWork = unitOfWork;
            _passwordHasher = passwordHasher;
            _authorizationProvider = authorizationProvider;
            _emailSender = emailSender;
            _tokenService = tokenService;
            _tokenHandler = tokenHandler;
            _userService = userService;
            _logger = logger;
            _errors = errors.Value;
        }

        public async Task<SocialUser> RegisterAsync(SocialRegisterRequest registerRequest)
        {
            var user = _userService.GetExistingUser(registerRequest);
            SocialUser socialUser = new SocialUser()
            {
                User = user,
                UserId = user.Id,
                Provider = registerRequest.Provider,
                SocialId = registerRequest.SocialId
            };

            _unitOfWork.SocialUser.Add(socialUser);
            await _unitOfWork.CommitAsync();

            return _unitOfWork.SocialUser.Get(registerRequest.SocialId, registerRequest.Provider);
        }        

        public SocialUser Authenticate(SocialAuthenticationRequest credentials)
        {
            SocialUser socialUser = _unitOfWork.SocialUser.Get(credentials.SocialId, credentials.Provider);

            if (socialUser == null)
            {
                var error = _errors[BusinessErrorType.UserNotFound];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }

            return socialUser;
        }
    }
}