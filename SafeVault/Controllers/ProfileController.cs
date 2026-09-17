using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SafeVault.Data;

namespace SafeVault.Controllers;

[Authorize]
public sealed class ProfileController(SafeVaultDbContext db) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Forbid();
        }

        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(item => item.UserId == userId, cancellationToken);
        return user is null ? NotFound() : View(user);
    }
}
