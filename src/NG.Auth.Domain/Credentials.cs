using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class Credentials
    {
        public Credentials(string EmailAddress, string Password)
        {
            this.EmailAddress = EmailAddress;
            this.Password = Password;
        }

        [Required]
        [EmailAddress]
        public string EmailAddress { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
