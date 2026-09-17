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

# Security and Authentication Summary

## Vulnerabilities Identified

The Activity 3 security audit found no exploitable SQL injection, XSS, plaintext-password, authentication-bypass, authorization, or input-validation vulnerability in the current implementation. The review confirmed that the existing protections are applied in the server-side code paths.

The audit also verified that login rejects external return URLs through `Url.IsLocalUrl`, preventing an open-redirect condition. This was covered by a regression test; no production fix was required.

## Fixes Applied

- Registration keeps server-side validation through `RegisterViewModel` and `RegistrationValidator`.
- Database access uses EF Core LINQ expressions in `UserService`, `ProfileController`, and `AdminController`; user input is not concatenated into SQL.
- Passwords are hashed with ASP.NET Core `PasswordHasher<User>` before persistence.
- Razor views render user-controlled values with normal Razor HTML encoding and do not use `Html.Raw`.
- Authentication uses the `SafeVaultCookie` cookie scheme.
- Profile access requires authentication, and the Admin dashboard requires the `Admin` role.
- Activity 3 added an integration regression test for external return URL rejection.

## Authentication and Authorization

Users register with validated usernames, email addresses, and passwords. Login accepts a username or email and verifies the supplied password against the stored hash. Authentication is established with the `SafeVaultCookie` ASP.NET Core cookie scheme.

Authorization is enforced server-side. The `User` role can access normal authenticated functionality, while the `Admin` role can also access the protected Admin dashboard. Admin users are seeded only when administrator configuration values are provided.

## Security Testing

`SecurityTests` covers server-side input validation, password hashing, SQL injection-shaped input, safe EF Core lookup behavior, and XSS HTML encoding. `AuthenticationAuthorizationTests` uses a real ASP.NET Core test host and temporary SQLite database to cover registration, successful and failed login, authenticated profile access, anonymous access denial, User denial of Admin access, Admin access, and external return URL rejection.

The complete solution builds successfully. The final NUnit run completed with 20 passed, 0 failed, and 0 skipped tests.

## How GitHub Copilot Assisted

GitHub Copilot assisted with code analysis, secure implementation guidance, and reviewed changes for:

- Input validation in `AccountViewModels` and `RegistrationValidator`
- SQL injection prevention through EF Core LINQ usage
- Authentication and password hashing in `AccountController` and `PasswordService`
- Authorization and RBAC in `ProfileController` and `AdminController`
- XSS review of Razor views
- NUnit security and integration test generation
- Debugging compile and test-runner issues

All changes were reviewed against the existing project architecture. The test fixture required a cleanup fix so temporary SQLite files are released after the host shuts down; the final suite then passed.

## Rubric Evidence

| Requirement | Evidence |
| --- | --- |
| GitHub repository | Git repository containing `SafeVault.slnx`, SafeVault source, `SafeVault.Tests`, and `README.md` |
| Input validation | [SafeVault/Models/AccountViewModels.cs](SafeVault/Models/AccountViewModels.cs), [SafeVault/Services/RegistrationValidator.cs](SafeVault/Services/RegistrationValidator.cs), [SafeVault/Services/UserService.cs](SafeVault/Services/UserService.cs) |
| SQL injection prevention | [SafeVault/Services/UserService.cs](SafeVault/Services/UserService.cs), [SafeVault/Controllers/ProfileController.cs](SafeVault/Controllers/ProfileController.cs), [SafeVault/Controllers/AdminController.cs](SafeVault/Controllers/AdminController.cs), [SafeVault/Data/SafeVaultDbContext.cs](SafeVault/Data/SafeVaultDbContext.cs) |
| Authentication | [SafeVault/Program.cs](SafeVault/Program.cs), [SafeVault/Controllers/AccountController.cs](SafeVault/Controllers/AccountController.cs), [SafeVault/Services/PasswordService.cs](SafeVault/Services/PasswordService.cs) |
| Authorization/RBAC | [SafeVault/Controllers/ProfileController.cs](SafeVault/Controllers/ProfileController.cs), [SafeVault/Controllers/AdminController.cs](SafeVault/Controllers/AdminController.cs), [SafeVault/Models/User.cs](SafeVault/Models/User.cs), [SafeVault/Views/Admin/Index.cshtml](SafeVault/Views/Admin/Index.cshtml) |
| XSS protection | [SafeVault/Views/Profile/Index.cshtml](SafeVault/Views/Profile/Index.cshtml), [SafeVault/Views/Admin/Index.cshtml](SafeVault/Views/Admin/Index.cshtml), [SafeVault/Views/Account/Login.cshtml](SafeVault/Views/Account/Login.cshtml), [SafeVault/Views/Account/Register.cshtml](SafeVault/Views/Account/Register.cshtml) |
| Security tests | [SafeVault.Tests/SecurityTests.cs](SafeVault.Tests/SecurityTests.cs), [SafeVault.Tests/AuthenticationAuthorizationTests.cs](SafeVault.Tests/AuthenticationAuthorizationTests.cs) |
| Vulnerability fixes | [SafeVault/Services/UserService.cs](SafeVault/Services/UserService.cs), [SafeVault/Services/PasswordService.cs](SafeVault/Services/PasswordService.cs), [SafeVault/Controllers/AccountController.cs](SafeVault/Controllers/AccountController.cs), security tests in [SafeVault.Tests](SafeVault.Tests) |
| Copilot assistance | This README's activity summary and evidence documentation, supported by the implementation and test files listed above |

## Remaining Issues

- `dotnet build` reports a NuGet audit warning for the transitive package `SQLitePCLRaw.lib.e_sqlite3` version `2.1.11`.
