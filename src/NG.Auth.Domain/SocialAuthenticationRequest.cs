using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class SocialAuthenticationRequest
    {
        [Required]
        public Guid SocialId { get; set; }

        [Required]
        public string Provider { get; set; }
    }
}
