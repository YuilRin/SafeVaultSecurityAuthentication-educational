# SafeVault

A small ASP.NET Core MVC application that manages user accounts with SQLite persistence, secure password hashing, authentication, and role-based authorization.

## Course

Coursera - Security and Authentication

## Activities

### Activity 1

Secure coding, input validation, SQL injection prevention, XSS protection, and security testing.

### Activity 2

Authentication, authorization, password hashing, and RBAC.

### Activity 3

Security vulnerability identification, debugging, fixes, and security testing.

## Run locally

```powershell
dotnet run --project .\SafeVault\SafeVault.csproj
```

The application creates `safevault.db` on first run. To seed an administrator, set `AdminSeed:Username`, `AdminSeed:Email`, and `AdminSeed:Password` configuration values, for example with `AdminSeed__Username`, `AdminSeed__Email`, and `AdminSeed__Password` environment variables, before starting the application.

Run the test suite with:

```powershell
dotnet test .\SafeVault.slnx
```
