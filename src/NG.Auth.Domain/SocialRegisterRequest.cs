using NG.DBManager.Infrastructure.Contracts.Models.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace NG.Auth.Domain
{
    public class SocialRegisterRequest : CommonRegisterFields
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        public string SocialId { get; set; }
    }
}
