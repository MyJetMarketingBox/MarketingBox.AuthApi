using MarketingBox.Auth.Service.Crypto;
using MarketingBox.Auth.Service.Grpc;
using MarketingBox.Auth.Service.MyNoSql.Users;
using MarketingBox.AuthApi.Domain.Models.Errors;
using Microsoft.IdentityModel.Tokens;
using MyNoSqlServer.Abstractions;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Mail;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using MarketingBox.Auth.Service.Grpc.Models;

namespace MarketingBox.AuthApi.Domain.Tokens
{
    public class TokensService : ITokensService
    {
        private const string UserIdClaim = "user-id";
        private const string UserNameClaim = "user-name";
        private const string TenantIdClaim = "tenant-id";


        private readonly IMyNoSqlServerDataReader<UserNoSql> _reader;
        private readonly IUserService _userService;
        private readonly ICryptoService _cryptoService;
        private readonly string _tokenSecret;
        private readonly string _mainAudience;
        private readonly TimeSpan _ttl;

        public TokensService(
            IMyNoSqlServerDataReader<UserNoSql> reader,
            IUserService userService,
            ICryptoService cryptoService,
            string tokenSecret,
            string mainAudience,
            TimeSpan ttl)
        {
            _reader = reader;
            _userService = userService;
            _cryptoService = cryptoService;
            _tokenSecret = tokenSecret;
            _mainAudience = mainAudience;
            _ttl = ttl;
        }

        public async Task<(TokenInfo Token, LoginError Error)> LoginAsync(string login, string tenantId, string password)
        {
            var isEmail = MailAddress.TryCreate(login, out var _);

            string passHash = null;
            string userSalt = null;
            string userName = null;

            var usersResponse = await _userService.SearchAsync(new SearchUserRequest()
            {
                Username = !isEmail ? login : null,
                Email = isEmail ? login : null,
                TenantId = tenantId
            });
            
            if (usersResponse?.Data == null || usersResponse.Data.Count == 0)
                return (null, new LoginError() { Type = LoginErrorType.NoUser });

            if (usersResponse.Data.Count > 1)
            {
                throw new InvalidOperationException("There can not be more than 1 user for tenant and login");
            }

            var user = usersResponse.Data.First();

            passHash = user.PasswordHash;
            userSalt = user.Salt;
            userName = user.Username;

            if (!_cryptoService.VerifyHash(userSalt, password, passHash))
            {
                return (null, new LoginError() { Type = LoginErrorType.WrongPassword });
            }

            var expAt = DateTime.UtcNow + _ttl;
            return (new TokenInfo() { 
                Token = Create(tenantId, userName, expAt, user.ExternalUserId), 
                ExpiresAt = expAt}, null);
        }

        private string Create(string tenantId, string username, DateTime expirationDate, string userId)
        {
            var properties = new Dictionary<string, string>
            {
                {UserNameClaim, username},
                {TenantIdClaim, tenantId},
                {UserIdClaim, userId},
            };

            var audiences = new List<string>()
            {
                _mainAudience
            };

            return Create(expirationDate, properties, audiences);
        }

        private string Create(DateTime expirationDate, IReadOnlyDictionary<string, string> properties,
            IEnumerable<string> audiences)
        {
            var key = Encoding.ASCII.GetBytes(_tokenSecret);

            var claims = audiences
                .Select(audience => new Claim(JwtRegisteredClaimNames.Aud, audience))
                .ToList();

            claims.AddRange(properties.Select(property => new Claim(property.Key, property.Value)));

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expirationDate,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }
    }
}