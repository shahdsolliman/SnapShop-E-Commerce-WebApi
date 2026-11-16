using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using SnapShop.Core.Entities.Identity;
using SnapShop.Infrastructure.Identity;
using System.Text;

namespace SnapShop.API.Extensions
{
    public static class IdentityServicesExtensions
    {
        public static IServiceCollection AddIdentityServices(
            this IServiceCollection services,
            IConfiguration configuration
            )
        {
            services.AddIdentity<AppUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<AppIdentityDbContext>();

            var jwtKey = configuration["JWT:key"];
            if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.StartsWith("SET_VIA") || Encoding.UTF8.GetByteCount(jwtKey) < 32)
            {
                throw new InvalidOperationException(
                    "JWT:key is not configured or is shorter than 32 bytes (256 bits). " +
                    "Please set a secure key via User Secrets in Development (`dotnet user-secrets set \"JWT:key\" \"<key>\"`) " +
                    "or via Environment Variable (`JWT__key`) in Production.");
            }

            var validIssuer = configuration["JWT:validIssuer"];
            if (string.IsNullOrWhiteSpace(validIssuer))
            {
                throw new InvalidOperationException(
                    "JWT:validIssuer is missing in configuration. " +
                    "Please specify a valid issuer URL in appsettings or environment variables.");
            }

            var validAudience = configuration["JWT:validAudience"];
            if (string.IsNullOrWhiteSpace(validAudience))
            {
                throw new InvalidOperationException(
                    "JWT:validAudience is missing in configuration. " +
                    "Please specify a valid audience in appsettings or environment variables.");
            }

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidIssuer = validIssuer,
                    ValidateAudience = true,
                    ValidAudience = validAudience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                    ClockSkew = TimeSpan.FromMinutes(5)
                };
            });
            return services;
        }
    }
}
