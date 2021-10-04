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
using System.Threading.Tasks;

namespace NG.Auth.Business.Impl
{
    public static class FirebaseAppService
    {
        public static FirebaseApp GetInstance()
        {
            if(FirebaseApp.DefaultInstance == null)
                FirebaseApp.Create(new AppOptions() { Credential = GoogleCredential.GetApplicationDefault() });

            return FirebaseApp.DefaultInstance;

        }
    }
}
