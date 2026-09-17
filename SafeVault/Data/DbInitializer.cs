using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SafeVault.Models;

namespace SafeVault.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        await using var scope = services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<SafeVaultDbContext>();
        await db.Database.EnsureCreatedAsync();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var username = configuration["AdminSeed:Username"];
        var email = configuration["AdminSeed:Email"];
        var password = configuration["AdminSeed:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password) ||
            await db.Users.AnyAsync(user => user.Role == "Admin"))
        {
            return;
        }

        var admin = new User
        {
            Username = username,
            Email = email,
            PasswordHash = string.Empty,
            Role = "Admin"
        };
        admin.PasswordHash = new PasswordHasher<User>().HashPassword(admin, password);
        db.Users.Add(admin);
        await db.SaveChangesAsync();
    }
}
