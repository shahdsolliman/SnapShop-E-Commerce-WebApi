using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapShop.API.MiddleWares;
using SnapShop.Core.Entities.Identity;
using SnapShop.Infrastructure.Data;
using SnapShop.Infrastructure.Identity;

namespace SnapShop.API.Extensions
{
    public static class ConfigureMiddleWares
    {
        public static async Task<WebApplication> ConfigureMiddleWaresAsync(this WebApplication app)
        {

            #region Update-Database Explicity

            using var scope = app.Services.CreateScope();

            var services = scope.ServiceProvider;

            var loggerFactory = services.GetRequiredService<ILoggerFactory>();
            var _dbContext = services.GetRequiredService<StoreContext>();
            var identityContext = services.GetRequiredService<AppIdentityDbContext>();
            var userManager = services.GetRequiredService<UserManager<AppUser>>();
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var configuration = services.GetRequiredService<IConfiguration>();

            try
            {
                await _dbContext.Database.MigrateAsync();
                await StoreContextSeed.SeedAsync(_dbContext);
                await identityContext.Database.MigrateAsync();

                var adminEmail = configuration["AdminUser:Email"];
                var adminPassword = configuration["AdminUser:Password"];
                await AppIdentityDbContextSeed.SeedRolesAndAdminAsync(roleManager, userManager, adminEmail, adminPassword);
            }
            catch (Exception ex)
            {

                var logger = loggerFactory.CreateLogger<Program>();
                logger.LogError(ex, "An error occurred during migration");
            }

            #endregion

            #region Configure Kestrel Middlewares

            app.UseMiddleware<ExceptionMiddleware>();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseStatusCodePagesWithReExecute("/errors/{0}");  // Not Found

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseCors("CorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers(); 

            #endregion

            return app;
        }
    }
}
