using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleCampus.Persistence
{
    public class Seed
    {
        public static async Task SeedData(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var admin = new IdentityUser
                {
                    Id = "admin",
                    UserName = "admin",
                };

                //TODO : Change this to use user secret
                await userManager.CreateAsync(admin, "adminPa$$w0rd");
            }
        }
    }
}
