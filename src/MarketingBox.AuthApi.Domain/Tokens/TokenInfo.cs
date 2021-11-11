using System;

namespace MarketingBox.AuthApi.Domain.Tokens
{
    public class TokenInfo
    {
        public string Token { get; set; }

        public DateTime ExpiresAt { get; set; }

        public Role Role { get; set; }
    }
}