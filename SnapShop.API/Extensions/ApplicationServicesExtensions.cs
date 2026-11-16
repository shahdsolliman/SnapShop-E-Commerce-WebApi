using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SnapShop.API.Errors;
using SnapShop.API.Helpers;
using SnapShop.Application;
using SnapShop.Core;
using SnapShop.Core.Repositories.Contract;
using SnapShop.Core.Services.Contract;
using SnapShop.Infrastructure;
using SnapShop.Infrastructure.Data;
using SnapShop.Infrastructure.Identity;
using StackExchange.Redis;

namespace SnapShop.API.Extensions
{
    public static class ApplicationServicesExtensions
    {

        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddBuiltInServices();
            services.AddSwaggerServices();
            services.AddDbContextServices(configuration);
            services.AddIdentityDbContextServices(configuration);
            services.AddUserDefinedServices();
            services.AddAutoMapperServices();
            services.ConfigureInValidResponseServices();
            services.AddRedisServices(configuration);

            var frontBaseUrl = configuration["FrontBaseUrl"];
            var configuredOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                ?? (!string.IsNullOrWhiteSpace(frontBaseUrl) ? new[] { frontBaseUrl } : Array.Empty<string>());

            var allowedOrigins = configuredOrigins
                .Where(o => !string.IsNullOrWhiteSpace(o))
                .Select(o => o.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray();

            if (allowedOrigins.Length == 0)
            {
                throw new InvalidOperationException(
                    "CORS: No allowed origins configured. " +
                    "Set 'FrontBaseUrl' or 'Cors:AllowedOrigins' in appsettings.json or environment variables.");
            }

            foreach (var origin in allowedOrigins)
            {
                if (!Uri.TryCreate(origin, UriKind.Absolute, out var uri))
                {
                    throw new InvalidOperationException(
                        $"CORS: Invalid origin '{origin}'. Origin must be a valid absolute URI (e.g. 'https://example.com').");
                }

                if (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps)
                {
                    throw new InvalidOperationException(
                        $"CORS: Invalid scheme '{uri.Scheme}' in origin '{origin}'. Only HTTP and HTTPS schemes are allowed.");
                }

                if (string.IsNullOrWhiteSpace(uri.Host))
                {
                    throw new InvalidOperationException(
                        $"CORS: Origin '{origin}' must contain a valid host.");
                }

                if (!string.IsNullOrEmpty(uri.UserInfo))
                {
                    throw new InvalidOperationException(
                        $"CORS: Origin '{origin}' must not contain user info.");
                }

                if (!string.IsNullOrEmpty(uri.Query))
                {
                    throw new InvalidOperationException(
                        $"CORS: Origin '{origin}' must not contain a query string.");
                }

                if (!string.IsNullOrEmpty(uri.Fragment))
                {
                    throw new InvalidOperationException(
                        $"CORS: Origin '{origin}' must not contain a fragment.");
                }

                if (uri.AbsolutePath != "/")
                {
                    throw new InvalidOperationException(
                        $"CORS: Origin '{origin}' must not contain a path.");
                }
            }

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            return services;
        }
        private static IServiceCollection AddBuiltInServices(this IServiceCollection services)
        {
            services.AddControllers();
            return services;
        }

        private static IServiceCollection AddSwaggerServices(this IServiceCollection services)
        {
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            return services;
        }

        private static IServiceCollection AddDbContextServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<StoreContext>(options => options.UseSqlServer(connectionString));

            return services;
        }
        private static IServiceCollection AddIdentityDbContextServices(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("IdentityConnection");
            if (string.IsNullOrWhiteSpace(connectionString) || connectionString.StartsWith("SET_VIA"))
            {
                // Fallback to DefaultConnection if only one database is provisioned (e.g. on shared hosting)
                connectionString = configuration.GetConnectionString("DefaultConnection");
            }
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(connectionString));

            return services;
        }

        private static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(c =>
            {
                var redisConn = configuration.GetConnectionString("Redis");
                if (string.IsNullOrWhiteSpace(redisConn))
                {
                    redisConn = "localhost:6379";
                }
                var connection = ConfigurationOptions.Parse(redisConn, true);
                connection.AbortOnConnectFail = false;
                return ConnectionMultiplexer.Connect(connection);
            });
            return services;
        }

        private static IServiceCollection AddUserDefinedServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddScoped(typeof(IUnitOfWork), typeof(UnitOfWork));
            services.AddScoped(typeof(IOrderService), typeof(OrderService));
            services.AddScoped(typeof(ITokenService), typeof(TokenService));
            services.AddScoped(typeof(IPaymentService), typeof(PaymentService));
            return services;
        }

        private static IServiceCollection AddAutoMapperServices(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MappingProfiles));

            return services;
        }
        private static IServiceCollection ConfigureInValidResponseServices(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
               options.InvalidModelStateResponseFactory = (ActionContext) =>
               {
                   var errors = ActionContext.ModelState.Where(P => P.Value.Errors.Count() > 0)
                                                        .SelectMany(E => E.Value.Errors)
                                                        .Select(E => E.ErrorMessage)
                                                        .ToArray();
                   var response = new ApiValidationErrorResponse
                   {
                       Errors = errors
                   };

                   return new BadRequestObjectResult(response);

               }
            );

            return services;
        }

    }
}
