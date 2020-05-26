using Microsoft.AspNetCore.Mvc.ModelBinding;
using NG.DBManager.Infrastructure.Contracts.Models;
using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using System;
using System.Threading.Tasks;

namespace NG.Auth.Domain
{
    public class RegBinder : IModelBinder
    {
        public Task BindModelAsync(ModelBindingContext bindingContext)
        {
            var values = bindingContext.ValueProvider;

            User user = new User()
            {
                Id = Guid.NewGuid(),
                Name = values.GetValue("Name").ToString(),
                Surname = values.GetValue("Surname").ToString(),
                PhoneNumber = values.GetValue("PhoneNumber").ToString(),
                //Birthdate = DateTime.Parse(values.GetValue("Birthdate").ToString()),
                Email = values.GetValue("Email").ToString(),
                Password = values.GetValue("Password").ToString(),
                Role = Role.Basic,
                Image = null,
            };

            bindingContext.Result = ModelBindingResult.Success(user);

            return Task.CompletedTask;
        }
    }
}
