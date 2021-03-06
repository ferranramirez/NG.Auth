using NG.Auth.Domain.Validations;
using System;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class StandardRegisterRequest : CommonRegisterFields
    {
        [Required]
        [MinLength(5)]
        public string Password { get; set; }
    }
}
