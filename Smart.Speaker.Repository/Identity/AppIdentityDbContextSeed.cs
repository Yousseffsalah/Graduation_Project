using Microsoft.AspNetCore.Identity;
using Smart.Speaker.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart.Speaker.Repository.Identity
{
    public static class AppIdentityDbContextSeed
    {
        public static async Task SeedUserAsync(UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var User = new AppUser()
                {
                    DisplayName = "Test User1",
                    Email = "testuser1@gmail.com",
                    PhoneNumber = "01273654921",
                    UserName = "testuser1"
                };

                await userManager.CreateAsync(User, "P@ssw0rd");
            }
        }
    }
}
