using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SnapShop.Core.Entities.Identity;
using System.Security.Claims;

namespace SnapShop.API.Extensions
{
    public static class UserManagerExtensions
    {
        public static async Task<AppUser> FindUserWithAddressByEmailAsync(this UserManager<AppUser> userManager, ClaimsPrincipal User)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            var user = await userManager.Users.Include(U => U.Address).SingleOrDefaultAsync(U => U.Email == email);
            return user;
        }
    }
}
