using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class Credentials
    {
        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
