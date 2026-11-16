using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SnapShop.Core.Entities.Identity;
using SnapShop.Core.Services.Contract;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Application
{
    public class TokenService : ITokenService
    {
        public TokenService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public async Task<string> CreateToken(AppUser user, UserManager<AppUser> userManager)
        {
            // User Claims
            var authClaims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.DisplayName)
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles) 
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Key Validation
            var jwtKey = Configuration["JWT:key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("SET_VIA") || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException(
                    "JWT:key is not configured or is shorter than 32 bytes (256 bits). " +
                    "Please set a secure key via User Secrets or Environment Variables.");
            }

            var validIssuer = Configuration["JWT:validIssuer"];
            if (string.IsNullOrWhiteSpace(validIssuer))
            {
                throw new InvalidOperationException("JWT:validIssuer is missing in configuration.");
            }

            var validAudience = Configuration["JWT:validAudience"];
            if (string.IsNullOrWhiteSpace(validAudience))
            {
                throw new InvalidOperationException("JWT:validAudience is missing in configuration.");
            }

            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var durationInDays = double.TryParse(Configuration["JWT:DurationInDays"], out var days) && days > 0 ? days : 7;

            var token = new JwtSecurityToken(
                issuer: validIssuer,
                audience: validAudience,
                expires: DateTime.UtcNow.AddDays(durationInDays),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    } 
}
