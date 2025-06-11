using SnapShop.API.Extensions;

namespace SnapShop.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var webApplicationBuilder = WebApplication.CreateBuilder(args);

            webApplicationBuilder.Configuration
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{webApplicationBuilder.Environment.EnvironmentName}.json", optional: true)
                .AddUserSecrets<Program>() 
                .AddEnvironmentVariables();

            webApplicationBuilder.Services.AddApplicationServices(webApplicationBuilder.Configuration);
            webApplicationBuilder.Services.AddIdentityServices(webApplicationBuilder.Configuration);

            var app = webApplicationBuilder.Build();
            await app.ConfigureMiddleWaresAsync();
             

            app.Run();
           
        }
    }
}
