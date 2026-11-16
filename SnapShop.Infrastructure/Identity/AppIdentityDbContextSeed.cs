using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapShop.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SnapShop.Infrastructure.Identity
{
    public class AppIdentityDbContextSeed
    {
        public static async Task SeedRolesAndAdminAsync(
            RoleManager<IdentityRole> roleManager,
            UserManager<AppUser> userManager,
            string? adminEmail = null,
            string? adminPassword = null)
        {
            // ── 1. Ensure roles exist ──
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // ── 2. Seed Admin user ──
            var email = !string.IsNullOrWhiteSpace(adminEmail) ? adminEmail : "admin@snapshop.com";
            var password = !string.IsNullOrWhiteSpace(adminPassword) ? adminPassword : "Admin$123";

            if (await userManager.FindByEmailAsync(email) == null)
            {
                var adminUser = new AppUser
                {
                    DisplayName = "Admin User",
                    Email = email,
                    UserName = email.Split('@')[0],
                    PhoneNumber = "01000000001"
                };
                var result = await userManager.CreateAsync(adminUser, password);
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, "Admin");
            }

            // ── 3. Seed demo customer user ──
            if (await userManager.FindByEmailAsync("demo@snapshop.com") == null)
            {
                var demoUser = new AppUser
                {
                    DisplayName = "Demo User",
                    Email = "demo@snapshop.com",
                    UserName = "demouser",
                    PhoneNumber = "01000000000"
                };
                var result = await userManager.CreateAsync(demoUser, "Pa$$w0rd");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(demoUser, "Customer");
            }
        }
    }
}
