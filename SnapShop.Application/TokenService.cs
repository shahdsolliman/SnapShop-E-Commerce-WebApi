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
            // Private Claimes -UserDefined-
            var authCalims = new List<Claim>()
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.DisplayName)
            };

            var userRoles = await userManager.GetRolesAsync(user);
            foreach (var role in userRoles) 
            {
                authCalims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Registered Claimes  --> appsettings

            // Key
            var authKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:key"]));

            var token = new JwtSecurityToken(
                issuer: Configuration["JWT:validIssuer"],
                audience: Configuration["JWT:validAudience"],
                expires: DateTime.Now.AddDays(double.Parse(Configuration["JWT:DurationInDays"])),
                claims: authCalims,
                signingCredentials: new SigningCredentials(authKey, SecurityAlgorithms.HmacSha256Signature)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    } 
}
