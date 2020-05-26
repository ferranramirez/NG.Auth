using Microsoft.AspNetCore.Mvc.ModelBinding;
using NG.DBManager.Infrastructure.Contracts.Models;

namespace NG.Auth.Presentation.WebAPI.ModelBinder
{
    public class RegisterBinderProvider : IModelBinderProvider
    {
        //private readonly IPasswordHasher _passwordHasher;

        //public RegisterBinderProvider(IPasswordHasher passwordHasher)
        //{
        //    _passwordHasher = passwordHasher;
        //}

        public IModelBinder GetBinder(ModelBinderProviderContext context)
        {
            if (context.Metadata.ModelType == typeof(User))
            {
                return new RegisterBinder();
            }

            return null;
        }
    }
}
