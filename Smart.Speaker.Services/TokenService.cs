using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Smart.Speaker.Core.Entities.Identity;
using Smart.Speaker.Core.Service;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Smart.Speaker.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string> CreateTokenAsync(AppUser user)
        {
            // PAYLOAD [Data] [Claims]

            // 1. Private Claims
            var AuthClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.GivenName, user.DisplayName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Security Key
            var AuthKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            // 2. Register Claims
            var Token = new JwtSecurityToken(issuer: _configuration["JWT:ValidIssure"],
                                             audience: _configuration["JWT:ValidAudience"],
                                             expires: DateTime.Now.AddDays(double.Parse(_configuration["JWT:DurationInDays"])),
                                             claims: AuthClaims,
                                             signingCredentials: new SigningCredentials(AuthKey, SecurityAlgorithms.Aes128CbcHmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(Token);
        }
    }
}
