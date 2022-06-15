using System;
using MarketingBox.AuthApi.Models.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MarketingBox.Auth.Service.Grpc;
using MarketingBox.Auth.Service.Grpc.Models;
using MarketingBox.AuthApi.Domain.Tokens;
using MarketingBox.Sdk.Common.Extensions;

namespace MarketingBox.AuthApi.Controllers
{
    [ApiController]
    [Route("/api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly ITokensService _tokensService;
        private readonly TenantLocator _tenantLocator;

        public AuthController(
            ITokensService tokensService,
            TenantLocator tenantLocator)
        {
            _tokensService = tokensService;
            _tenantLocator = tenantLocator;
        }

        /// <summary>
        /// </summary>
        /// <remarks>
        /// </remarks>
        [HttpPost("login")]
        [ProducesResponseType(typeof(AuthenticateResponse), StatusCodes.Status200OK)]

        public async Task<ActionResult<AuthenticateResponse>> LoginAsync(
            [FromBody] AuthenticateRequest request)
        {
            var tenantId = _tenantLocator.GetTenantIdByHost(Request.Host.Host);
            
            Console.WriteLine($"Get tenant '{tenantId}' from host '{Request.Host.Host}' ");
            
            request.ValidateEntity();

            var response= await _tokensService.LoginAsync(new TokenRequest
            {
                Login = request.Email,
                Password = request.Password,
                TenantId = tenantId
            });
            var token = response.Process();

            return Ok(new AuthenticateResponse()
            {
                Token = token.Token,
                ExpiresAt = token.ExpiresAt
            });
        }
    }
}