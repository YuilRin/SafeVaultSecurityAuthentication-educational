using Microsoft.EntityFrameworkCore;
using SafeVault.Models;

namespace SafeVault.Data;

public sealed class SafeVaultDbContext(DbContextOptions<SafeVaultDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(user => user.UserId);
            entity.Property(user => user.Username).HasMaxLength(32).IsRequired();
            entity.Property(user => user.Email).HasMaxLength(254).IsRequired();
            entity.Property(user => user.PasswordHash).IsRequired();
            entity.Property(user => user.Role).HasMaxLength(16).IsRequired();
            entity.HasIndex(user => user.Username).IsUnique();
            entity.HasIndex(user => user.Email).IsUnique();
        });
    }
}
