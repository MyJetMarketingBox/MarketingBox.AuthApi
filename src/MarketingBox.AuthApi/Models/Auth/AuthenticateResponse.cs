using System;

namespace MarketingBox.AuthApi.Models.Auth
{
    public class AuthenticateResponse
    {
        public string Token { get; set; }
        public DateTime ExpiresAt { get; set; }
        public Role Role { get; internal set; }
    }
}
