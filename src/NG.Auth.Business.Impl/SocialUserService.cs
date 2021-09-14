using FirebaseAdmin;
using FirebaseAdmin.Auth;
using Google.Apis.Auth.OAuth2;
using Microsoft.Extensions.Logging;
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
using System.Security.Claims;
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public class SocialUserService : ISocialUserService
    {
        private readonly IAuthUnitOfWork _unitOfWork;
        private readonly IUserService _userService;
        private readonly ILogger<SocialUserService> _logger;
        private readonly Dictionary<BusinessErrorType, BusinessErrorObject> _errors;

        public SocialUserService(
            IAuthUnitOfWork unitOfWork,
            IUserService userService,
            ILogger<SocialUserService> logger,
            IOptions<Dictionary<BusinessErrorType, BusinessErrorObject>> errors)
        {
            _unitOfWork = unitOfWork;
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

            await AddTokenClaimsAsync(registerRequest.SocialId);

            return _unitOfWork.SocialUser.Get(registerRequest.SocialId, registerRequest.Provider);
        }        

        private async Task AddTokenClaimsAsync(string uid)
        {
            FirebaseAppService.GetInstance();
            var userRecord = await FirebaseAuth.DefaultInstance.GetUserAsync(uid);
            SocialUser socialUser = _unitOfWork.SocialUser.Get(userRecord.Uid, userRecord.ProviderId);
            if (userRecord == null || socialUser == null)
            {
                var error = _errors[BusinessErrorType.UserNotFound];
                throw new NotGuiriBusinessException(error.Message, error.ErrorCode);
            }
            await FirebaseAuth.DefaultInstance.SetCustomUserClaimsAsync(uid, GetClaims(socialUser));
        }

        private static Dictionary<string, object> GetClaims(SocialUser socialUser)
        {
            return new Dictionary<string, object> {
                { ClaimTypes.NameIdentifier, socialUser.UserId },
                { ClaimTypes.Email, socialUser.User.Email },
                { ClaimTypes.Role, socialUser.User.Role }
            };
        }
    }
}
