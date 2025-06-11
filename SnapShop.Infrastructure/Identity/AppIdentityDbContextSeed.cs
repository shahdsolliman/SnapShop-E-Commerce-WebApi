using Microsoft.AspNetCore.Identity;
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
        public static async Task SeedUserAsync(UserManager<AppUser> userManager)
        {
            if(! userManager.Users.Any())
            {
                var user = new AppUser()
                {
                    DisplayName = "Shahd Soliman",
                    Email = "shahd.soliman2050@gmail.com",
                    UserName = "shahdsolliman",
                    PhoneNumber = "01065852613"
                };
                await userManager.CreateAsync(user, "Pa$$w0rd");
            }
        }
    }
}
