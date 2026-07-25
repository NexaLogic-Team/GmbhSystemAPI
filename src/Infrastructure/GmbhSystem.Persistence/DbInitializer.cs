using GmbhSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GmbhSystem.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<GmbhSystemDbContext>();

        await context.Database.MigrateAsync();

        if (!await context.Set<User>().AnyAsync(u => u.Username == "gmbh"))
        {
            var adminUser = new User
            {
                Username = "gmbh",
                Email = "gbmh@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123@gmbh.com"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            context.Set<User>().Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}