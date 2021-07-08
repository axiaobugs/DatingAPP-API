using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using DatingApp.Entities;

namespace DatingApp.Data
{
    public class Seed
    {
        public static async Task SeedUsers(DataContext context)
        {
            if (await context.Users.AnyAsync())
            {
                return;
            }

            var userDate = await System.IO.File.ReadAllTextAsync("Data/UserSeedData.json");
            var users = JsonSerializer.Deserialize<List<AppUser>>(userDate);
            foreach (var user in users)
            {
                user.UserName = user.UserName.ToLower();
                context.Add(user);
            }

            await context.SaveChangesAsync();
        }
    }
}
