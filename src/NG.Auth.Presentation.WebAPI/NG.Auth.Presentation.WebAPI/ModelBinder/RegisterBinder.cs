using Microsoft.AspNetCore.Mvc.ModelBinding;
using NG.Auth.Business.Contract.InternalServices;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using System;
using System.Globalization;
using System.Threading.Tasks;

namespace NG.Auth.Presentation.WebAPI.ModelBinder
{
    public class RegisterBinder : IModelBinder
    {
        private readonly IPasswordHasher _passwordHasher;

        public RegisterBinder(IPasswordHasher passwordHasher)
        {
            _passwordHasher = passwordHasher;
        }

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var values = bindingContext.ValueProvider;

            string password = values.GetValue("Password").FirstValue;
            string birthdate = values.GetValue("Birthdate").FirstValue;

            User user = new User()
            {
                Id = Guid.NewGuid(),
                Name = values.GetValue("Name").FirstValue,
                Surname = values.GetValue("Surname").FirstValue,
                Birthdate = DateTime.ParseExact(birthdate, "dd/MM/yyyy", CultureInfo.InvariantCulture),
                PhoneNumber = values.GetValue("PhoneNumber").FirstValue,
                Email = values.GetValue("Email").FirstValue,
                Password = _passwordHasher.Hash(password),
                Role = Role.Basic,
                Image = null,
            };

            bindingContext.Result = ModelBindingResult.Success(user);

            return Task.CompletedTask;
        }
    }
}