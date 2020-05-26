using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using NG.Auth.Business.Contract.InternalServices;
using NG.DBManager.Infrastructure.Contracts.Models;

namespace NG.Auth.Presentation.WebAPI.ModelBinder
{
    public class RegisterBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(User))
            {
                var passwordHasher = context.Services.GetService<IPasswordHasher>();
                return new RegisterBinder(passwordHasher);
            }

            return null;
        }
    }
}
