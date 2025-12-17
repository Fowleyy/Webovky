using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Presentation.Controllers;

public class AdminController : Controller
{
    private readonly CalendarDbContext _db;

    public AdminController(CalendarDbContext db)
    {
        _db = db;
    }

    // =========================
    // HELPERS
    // =========================
    private bool IsAdmin =>
        HttpContext.Session.GetString("isAdmin") == "1";

    private IActionResult? RequireAdmin()
    {
        if (!IsAdmin)
            return Redirect("/login");
        return null;
    }

    // =========================
    // GET /admin
    // =========================
    public async Task<IActionResult> Index(string? search)
    {
        var check = RequireAdmin();
        if (check != null) return check;

        var users = _db.Users.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            string s = search.ToLower();
            users = users.Where(u =>
                (u.Email ?? "").ToLower().Contains(s) ||
                (u.FullName ?? "").ToLower().Contains(s));
        }

        ViewBag.Search = search;
        return View(await users.ToListAsync());
    }

    public async Task<IActionResult> Calendar(Guid id)
    {
        var check = RequireAdmin();
        if (check != null) return check;

        // 🔥 NAJDI KALENDÁŘ PODLE OWNERA
        var calendar = await _db.Calendars
            .Include(c => c.Events)
            .Include(c => c.Owner)
            .FirstOrDefaultAsync(c => c.OwnerId == id);

        if (calendar == null)
            return NotFound("Kalendář neexistuje.");

        return View("AdminCalendar", calendar);
    }

    [HttpPost]
    public async Task<IActionResult> DeleteUser(Guid id)
    {
        var check = RequireAdmin();
        if (check != null) return check;

        var user = await _db.Users
            .Include(u => u.Calendars)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
            return NotFound();

        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return Redirect("/admin");
    }

    [HttpPost]
    public async Task<IActionResult> ToggleAdmin(Guid id)
    {
        var check = RequireAdmin();
        if (check != null) return check;

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound();

        user.IsAdmin = !user.IsAdmin;
        await _db.SaveChangesAsync();

        return Redirect("/admin");
    }
}
