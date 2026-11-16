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

            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy", policy =>
                {
                    var frontBaseUrl = configuration["FrontBaseUrl"];
                    var allowedOrigins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
                        ?? (!string.IsNullOrWhiteSpace(frontBaseUrl) ? new[] { frontBaseUrl } : Array.Empty<string>());

                    if (allowedOrigins.Length > 0)
                    {
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                    else
                    {
                        policy.SetIsOriginAllowed(_ => true)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
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
            services.AddDbContext<StoreContext>(options => options/*.UseLazyLoadingProxies()*/.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
        private static IServiceCollection AddIdentityDbContextServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppIdentityDbContext>(options => options.UseSqlServer(configuration.GetConnectionString("IdentityConnection")));

            return services;
        }

        private static IServiceCollection AddRedisServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IConnectionMultiplexer>(c =>
            {
                var connection = ConfigurationOptions.Parse(configuration.GetConnectionString("Redis"), true);
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
