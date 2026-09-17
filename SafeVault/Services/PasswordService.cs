using Microsoft.AspNetCore.Identity;
using SafeVault.Models;

namespace SafeVault.Services;

public sealed class PasswordService(IPasswordHasher<User> passwordHasher)
{
    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string password, string passwordHash) =>
        passwordHasher.VerifyHashedPassword(user, passwordHash, password) != PasswordVerificationResult.Failed;
}
