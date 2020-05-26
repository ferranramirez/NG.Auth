using Microsoft.AspNetCore.Mvc.ModelBinding;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using System;
using System.Threading.Tasks;

namespace NG.Auth.Domain
{
    public class RegBinder : IModelBinder
    {
        //private readonly IPasswordHasher _passwordHasher;

        //public RegisterBinder(IPasswordHasher passwordHasher)
        //{
        //    _passwordHasher = passwordHasher;
        //}

        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var values = bindingContext.ValueProvider;

            string password = values.GetValue("Password").ToString();
            User user = new User()
            {
                Id = Guid.NewGuid(),
                Name = values.GetValue("Name").ToString(),
                Surname = values.GetValue("Surname").ToString(),
                Birthdate = DateTime.Parse(values.GetValue("Birthdate").ToString()),
                PhoneNumber = values.GetValue("PhoneNumber").ToString(),
                Email = values.GetValue("Email").ToString(),
                Password = password, // _passwordHasher.Hash(password),
                Role = Role.Basic,
                Image = null,
            };

            bindingContext.Result = ModelBindingResult.Success(user);

            return Task.CompletedTask;
        }
    }
}
