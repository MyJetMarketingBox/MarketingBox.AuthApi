using System.ComponentModel.DataAnnotations;
using MarketingBox.Sdk.Common.Models;

namespace MarketingBox.AuthApi.Models.Auth
{
    public class AuthenticateRequest : ValidatableEntity
    {
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
