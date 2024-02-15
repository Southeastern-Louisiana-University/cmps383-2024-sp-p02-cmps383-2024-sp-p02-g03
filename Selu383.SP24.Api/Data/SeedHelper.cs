using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Selu383.SP24.Api.Features.Users;

namespace Selu383.SP24.Api.Data
{
    public class SeedHelper
    {
        public static async Task MigrateAndSeed(IServiceProvider serviceProvider)
        {
            var dataContext = serviceProvider.GetRequiredService<DataContext>();
            await dataContext.Database.MigrateAsync();

            await AddRoles(serviceProvider);
            await AddUsers(serviceProvider);
        }
        public static async Task AddUsers(IServiceProvider serviceProvider)
        {
            const string defaultPassword = "Password123!";
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();

            if (userManager.Users.Any())
            {
                return;
            }

            var adminUser = new User
            {
                UserName = "galkadi",
            };

            var result = await userManager.CreateAsync(adminUser, defaultPassword);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(adminUser, Rnames.Admin);
            }

            var bob = new User
            {
                UserName = "bob"
            };

            await userManager.CreateAsync(bob, defaultPassword);
            await userManager.AddToRoleAsync(bob, Rnames.User);

            var sue = new User
            {
                UserName = "sue"
            };

            await userManager.CreateAsync(sue, defaultPassword);
            await userManager.AddToRoleAsync(sue, Rnames.User);
        }
        public static async Task AddRoles(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<Role>>();

            if (roleManager.Roles.Any())
            {
                return;
            }

            await roleManager.CreateAsync(new Role { Name = Rnames.Admin, });
            await roleManager.CreateAsync(new Role { Name = Rnames.User, });
        }
    }
}
