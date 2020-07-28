using NG.Auth.Domain.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class RegisterRequest
    {
        [Required]
        public string Name { get; set; }

        [BirthdateValidator(MinAge = 5, MaxAge = 120,
            ErrorMessage = "You must be between 6 and 119 years old to register")]
        public DateTime Birthdate { get; set; }

        //[RegularExpression("\\+(9[976]\\d|8[987530]\\d|6[987]\\d|5[90]\\d|42\\d|3[875]\\d|2[98654321]\\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|4[987654310]|3[9643210]|2[70]|7|1)\\d{1,14}$",
        //    ErrorMessage = "Incorrect phone number format")]
        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Wrong Email address")]
        public string Email { get; set; }

        [Required]
        [MinLength(5)]
        public string Password { get; set; }

        public bool IsCommerce { get; set; }
    }
}
