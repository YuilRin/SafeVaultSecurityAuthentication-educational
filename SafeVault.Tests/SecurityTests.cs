using System.ComponentModel.DataAnnotations;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SafeVault.Data;
using SafeVault.Models;
using SafeVault.Services;

namespace SafeVault.Tests;

[TestFixture]
public sealed class SecurityTests
{
    private SqliteConnection connection = null!;
    private SafeVaultDbContext db = null!;
    private UserService userService = null!;

    [SetUp]
    public async Task SetUp()
    {
        connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();
        var options = new DbContextOptionsBuilder<SafeVaultDbContext>()
            .UseSqlite(connection)
            .Options;
        db = new SafeVaultDbContext(options);
        await db.Database.EnsureCreatedAsync();
        var passwordService = new PasswordService(new PasswordHasher<User>());
        userService = new UserService(db, passwordService);
    }

    [TearDown]
    public async Task TearDown()
    {
        await db.DisposeAsync();
        await connection.DisposeAsync();
    }

    [Test]
    public void ValidUsernamePassesServerValidation()
    {
        Assert.That(RegistrationValidator.IsValid("alice_01", "alice@example.com", "StrongPassword1!"), Is.True);
    }

    [Test]
    public void EmptyRequiredFieldsFailServerValidation()
    {
        Assert.That(RegistrationValidator.IsValid(string.Empty, "alice@example.com", "StrongPassword1!"), Is.False);
        Assert.That(RegistrationValidator.IsValid("alice", string.Empty, "StrongPassword1!"), Is.False);
        Assert.That(RegistrationValidator.IsValid("alice", "alice@example.com", string.Empty), Is.False);
    }

    [Test]
    public void ValidRegistrationInputPassesServerValidation()
    {
        Assert.That(RegistrationValidator.IsValid("alice_01", "alice@example.com", "StrongPassword1!"), Is.True);
    }

    [Test]
    public void InvalidUsernameFailsServerValidation()
    {
        Assert.That(RegistrationValidator.IsValid("ab", "alice@example.com", "StrongPassword1!"), Is.False);
        Assert.That(RegistrationValidator.IsValid("<script>", "alice@example.com", "StrongPassword1!"), Is.False);
    }

    [Test]
    public void InvalidEmailFailsServerValidation()
    {
        Assert.That(RegistrationValidator.IsValid("alice", "not-an-email", "StrongPassword1!"), Is.False);
    }

    [Test]
    public void InvalidPasswordFailsServerValidation()
    {
        Assert.That(RegistrationValidator.IsValid("alice", "alice@example.com", "short"), Is.False);
        Assert.That(RegistrationValidator.IsValid("alice", "alice@example.com", "alllowercasepassword1!"), Is.False);
    }

    [Test]
    public async Task RegistrationRejectsSqlInjectionShapedUsername()
    {
        var result = await userService.RegisterAsync("' OR '1'='1", "alice@example.com", "StrongPassword1!");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(await db.Users.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task RegistrationRejectsXssShapedUsername()
    {
        var result = await userService.RegisterAsync("<script>alert('XSS')</script>", "alice@example.com", "StrongPassword1!");

        Assert.That(result.Succeeded, Is.False);
        Assert.That(await db.Users.CountAsync(), Is.Zero);
    }

    [Test]
    public async Task RegistrationStoresAHashAndVerifiesTheOriginalPassword()
    {
        var result = await userService.RegisterAsync("alice", "alice@example.com", "StrongPassword1!");
        var user = await db.Users.SingleAsync();
        var passwordService = new PasswordService(new PasswordHasher<User>());

        Assert.That(result.Succeeded, Is.True);
        Assert.That(user.PasswordHash, Is.Not.EqualTo("StrongPassword1!"));
        Assert.That(passwordService.Verify(user, "StrongPassword1!", user.PasswordHash), Is.True);
        Assert.That(passwordService.Verify(user, "WrongPassword1!", user.PasswordHash), Is.False);
    }

    [Test]
    public async Task UserLookupTreatsSqlInjectionTextAsData()
    {
        await userService.RegisterAsync("alice", "alice@example.com", "StrongPassword1!");

        var user = await userService.FindByUsernameAsync("' OR '1'='1");

        Assert.That(user, Is.Null);
        Assert.That(await db.Users.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public void RazorHtmlEncoderEncodesAnXssPayload()
    {
        const string payload = "<script>alert('xss')</script>";

        var encoded = HtmlEncoder.Default.Encode(payload);

        Assert.That(encoded, Is.EqualTo("&lt;script&gt;alert(&#x27;xss&#x27;)&lt;/script&gt;"));
        Assert.That(encoded, Does.Not.Contain("<script>"));
    }

    [Test]
    public void RegistrationViewModelUsesTheSameServerSideRules()
    {
        var model = new RegisterViewModel
        {
            Username = "alice",
            Email = "not-an-email",
            Password = "StrongPassword1!"
        };
        var results = new List<ValidationResult>();

        var valid = Validator.TryValidateObject(model, new ValidationContext(model), results, true);

        Assert.That(valid, Is.False);
        Assert.That(results, Has.Some.Matches<ValidationResult>(result => result.ErrorMessage == "Enter a valid email address."));
    }
}
