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
        public Guid SocialId { get; set; }
    }
}
