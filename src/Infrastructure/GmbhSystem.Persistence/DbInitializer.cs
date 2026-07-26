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
                Id = Guid.NewGuid().ToString(),
                Username = "gmbh",
                Email = "gbmh@gmail.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("123@gmbh.com"),
                Role = "Admin",
                CreatedAt = DateTime.UtcNow
            };

            context.Set<User>().Add(adminUser);
            await context.SaveChangesAsync();
        }
        if (!context.ContentItems.Any(c => c.Section == "Home"))
        {
            var defaultContents = new List<ContentItem>
            {
                // English (en)
                new ContentItem { Key = "MainTitle", Value = "Connecting Germany and Myanmar Through Business, Trade, and Innovation", Section = "Home", Language = "en" },
                new ContentItem { Key = "DescriptionParagraph", Value = "Irrawaddy GmbH is a Munich-based company dedicated to creating business opportunities between Germany and Myanmar.", Section = "Home", Language = "en" },
            
                // German (de)
                new ContentItem { Key = "MainTitle", Value = "Verbindung von Deutschland und Myanmar durch Wirtschaft, Handel und Innovation", Section = "Home", Language = "de" },
                new ContentItem { Key = "DescriptionParagraph", Value = "Die Irrawaddy GmbH ist ein in München ansässiges Unternehmen, das sich der Schaffung von Geschäftsmöglichkeiten zwischen Deutschland und Myanmar widmet.", Section = "Home", Language = "de" }
            };

            context.ContentItems.AddRange(defaultContents);
            await context.SaveChangesAsync();
        }
    }
}