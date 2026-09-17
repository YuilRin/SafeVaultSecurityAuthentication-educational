using Microsoft.EntityFrameworkCore;
using SafeVault.Data;
using SafeVault.Models;

namespace SafeVault.Services;

public sealed class UserService(SafeVaultDbContext db, PasswordService passwordService)
{
    public async Task<(bool Succeeded, string? Error)> RegisterAsync(
        string username, string email, string password, CancellationToken cancellationToken = default)
    {
        username = username.Trim();
        email = email.Trim();

        if (!RegistrationValidator.IsValid(username, email, password))
        {
            return (false, "Registration details are invalid.");
        }

        if (await db.Users.AnyAsync(user => user.Username == username || user.Email == email, cancellationToken))
        {
            return (false, "That username or email is already registered.");
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = string.Empty,
            Role = "User"
        };
        user.PasswordHash = passwordService.Hash(user, password);
        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return (true, null);
    }

    public Task<User?> FindByUsernameOrEmailAsync(string usernameOrEmail, CancellationToken cancellationToken = default) =>
        db.Users.SingleOrDefaultAsync(
            user => user.Username == usernameOrEmail || user.Email == usernameOrEmail,
            cancellationToken);
}
