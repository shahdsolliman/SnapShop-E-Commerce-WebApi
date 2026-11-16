using System.Reflection;
using Microsoft.OpenApi.Models;
using Serilog;
using SnapShop.API.Extensions;

namespace SnapShop.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var webApplicationBuilder = WebApplication.CreateBuilder(args);

            // Configure Serilog
            webApplicationBuilder.Host.UseSerilog((ctx, lc) => 
                lc.ReadFrom.Configuration(ctx.Configuration));

            // Configuration sources
            webApplicationBuilder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Program>(optional: true)
                .AddEnvironmentVariables();

            // Register services
            webApplicationBuilder.Services.AddApplicationServices(webApplicationBuilder.Configuration);
            webApplicationBuilder.Services.AddIdentityServices(webApplicationBuilder.Configuration);

            webApplicationBuilder.Services.AddHealthChecks();
            webApplicationBuilder.Services.AddEndpointsApiExplorer();

            // Swagger configuration with JWT Bearer support
            webApplicationBuilder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "SnapShop API",
                    Version = "v1",
                    Description = "E-Commerce backend API built with Clean Architecture"
                });

                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                if (File.Exists(xmlPath))
                    c.IncludeXmlComments(xmlPath);
            });

            var app = webApplicationBuilder.Build();

            // Run database migrations & identity seed
            await app.ConfigureMiddleWaresAsync();

            // Serilog request logging
            app.UseSerilogRequestLogging();

            // Map Health Checks endpoint
            app.MapHealthChecks("/health");

            app.Run();
        }
    }
}
