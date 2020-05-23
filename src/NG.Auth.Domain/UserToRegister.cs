using System;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class UserToRegister
    {
        [Required]
        public string Name { get; set; }
        [Required]

        public string Surname { get; set; }
        public DateTime Birthdate { get; set; }

        public string PhoneNumber { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
