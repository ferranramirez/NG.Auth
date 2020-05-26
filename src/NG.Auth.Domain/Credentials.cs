using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class Credentials : IValidatableObject
    {
        [EmailAddress]
        public string EmailAddress { get; set; }

        [RegularExpression("\\+(9[976]\\d|8[987530]\\d|6[987]\\d|5[90]\\d|42\\d|3[875]\\d|2[98654321]\\d|9[8543210]|8[6421]|6[6543210]|5[87654321]|4[987654310]|3[9643210]|2[70]|7|1)\\d{1,14}$",
            ErrorMessage = "Incorrect phone number format")]
        public string PhoneNumber { get; set; }

        [Required]
        [MinLength(5)]
        public string Password { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var errors = new List<ValidationResult>();

            if (string.IsNullOrEmpty(EmailAddress)
                && string.IsNullOrEmpty(PhoneNumber))
            {
                errors.Add(new ValidationResult("Email address or phone number required"));
            }

            return errors;
        }
    }
}
