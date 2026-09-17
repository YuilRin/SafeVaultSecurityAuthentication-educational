using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using SafeVault.Data;

namespace SafeVault.Tests;

[TestFixture]
[NonParallelizable]
public sealed class AuthenticationAuthorizationTests
{
    private SafeVaultWebApplicationFactory factory = null!;
    private HttpClient client = null!;

    [SetUp]
    public void SetUp()
    {
        factory = new SafeVaultWebApplicationFactory();
        client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false,
            BaseAddress = new Uri("https://localhost")
        });
    }

    [TearDown]
    public void TearDown()
    {
        client.Dispose();
        factory.Dispose();
    }

    [Test]
    public async Task SuccessfulRegistrationRedirectsToLogin()
    {
        var response = await RegisterAsync("alice", "alice@example.com", "StrongPassword1!");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/Login"));
    }

    [Test]
    public async Task SuccessfulLoginAuthenticatesTheUser()
    {
        await RegisterAsync("alice", "alice@example.com", "StrongPassword1!");

        var response = await LoginAsync("alice@example.com", "StrongPassword1!");
        var profileResponse = await client.GetAsync("/Profile");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(profileResponse.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(await profileResponse.Content.ReadAsStringAsync(), Does.Contain("alice"));
    }

    [Test]
    public async Task FailedLoginWithIncorrectPasswordDoesNotAuthenticate()
    {
        await RegisterAsync("alice", "alice@example.com", "StrongPassword1!");

        var response = await LoginAsync("alice", "WrongPassword1!");
        var profileResponse = await client.GetAsync("/Profile");
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain("Invalid username or password."));
        Assert.That(profileResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
    }

    [Test]
    public async Task UnauthenticatedUserIsRedirectedFromProtectedProfile()
    {
        var response = await client.GetAsync("/Profile");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/Login"));
    }

    [Test]
    public async Task AuthenticatedUserCanAccessProfile()
    {
        await RegisterAsync("alice", "alice@example.com", "StrongPassword1!");
        await LoginAsync("alice", "StrongPassword1!");

        var response = await client.GetAsync("/Profile");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
    }

    [Test]
    public async Task AuthenticatedUserIsDeniedAdminAccess()
    {
        await RegisterAsync("alice", "alice@example.com", "StrongPassword1!");
        await LoginAsync("alice", "StrongPassword1!");

        var response = await client.GetAsync("/Admin");

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.ToString(), Does.Contain("/Account/AccessDenied"));
    }

    [Test]
    public async Task AdminCanAccessAdminDashboard()
    {
        await LoginAsync("admin", "AdminPassword1!");

        var response = await client.GetAsync("/Admin");
        var body = await response.Content.ReadAsStringAsync();

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        Assert.That(body, Does.Contain("Admin"));
        Assert.That(body, Does.Contain("admin"));
    }

    private async Task<HttpResponseMessage> RegisterAsync(string username, string email, string password)
    {
        var token = await GetAntiForgeryTokenAsync("/Account/Register");
        return await client.PostAsync("/Account/Register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["Username"] = username,
            ["Email"] = email,
            ["Password"] = password
        }));
    }

    private async Task<HttpResponseMessage> LoginAsync(string usernameOrEmail, string password)
    {
        var token = await GetAntiForgeryTokenAsync("/Account/Login");
        return await client.PostAsync("/Account/Login", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["UsernameOrEmail"] = usernameOrEmail,
            ["Password"] = password
        }));
    }

    private async Task<string> GetAntiForgeryTokenAsync(string path)
    {
        var response = await client.GetAsync(path);
        var html = await response.Content.ReadAsStringAsync();
        var match = Regex.Match(html, "name=\"__RequestVerificationToken\"[^>]*value=\"([^\"]+)\"");
        Assert.That(match.Success, Is.True, $"No antiforgery token was found in {path}.");
        return match.Groups[1].Value;
    }

    private sealed class SafeVaultWebApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string databasePath = Path.Combine(Path.GetTempPath(), $"safevault-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:SafeVault"] = $"Data Source={databasePath}",
                    ["AdminSeed:Username"] = "admin",
                    ["AdminSeed:Email"] = "admin@example.com",
                    ["AdminSeed:Password"] = "AdminPassword1!"
                });
            });
            builder.ConfigureServices(services =>
            {
                var descriptor = services.Single(item => item.ServiceType == typeof(DbContextOptions<SafeVaultDbContext>));
                services.Remove(descriptor);
                services.AddDbContext<SafeVaultDbContext>(options => options.UseSqlite($"Data Source={databasePath}"));
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && File.Exists(databasePath))
            {
                File.Delete(databasePath);
            }
        }
    }
}
