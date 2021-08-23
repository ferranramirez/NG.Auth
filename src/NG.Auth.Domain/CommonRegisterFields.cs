using NG.Auth.Domain.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class CommonRegisterFields
    {
        [Required]
        public string Name { get; set; }

        [BirthdateValidator(MinAge = 5, MaxAge = 120,
            ErrorMessage = "You must be between 6 and 119 years old to register")]
        public DateTime Birthdate { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Wrong Email address")]
        public string Email { get; set; }

        public bool IsCommerce { get; set; }
    }
}
