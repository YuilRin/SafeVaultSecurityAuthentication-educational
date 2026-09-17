using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeVault.Data;

namespace SafeVault.Controllers;

[Authorize(Roles = "Admin")]
public sealed class AdminController(SafeVaultDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken) =>
        View(await db.Users.AsNoTracking().OrderBy(user => user.Username).ToListAsync(cancellationToken));
}
