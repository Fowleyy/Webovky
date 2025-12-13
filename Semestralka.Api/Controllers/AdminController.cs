using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    public class AdminController : Controller
    {
        private readonly CalendarDbContext _db;

        public AdminController(CalendarDbContext db)
        {
            _db = db;
        }

        private bool IsAdmin =>
            HttpContext.Session.GetString("isAdmin") == "1";

        private IActionResult? RequireAdmin()
        {
            if (!IsAdmin)
                return Unauthorized();
            return null;
        }

        [HttpGet("/admin")]
        public async Task<IActionResult> Index(string? search)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var users = _db.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                string s = search.ToLower();
                users = users.Where(x =>
                    (x.Email ?? "").ToLower().Contains(s) ||
                    (x.FullName ?? "").ToLower().Contains(s));
            }

            ViewBag.Search = search;
            return View(await users.ToListAsync());
        }

        [HttpGet("/admin/delete-user/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
                return NotFound();

            _db.Users.Remove(user);
            await _db.SaveChangesAsync();

            return Redirect("/admin");
        }

        [HttpGet("/admin/calendar/{userId}")]
        public async Task<IActionResult> UserCalendar(Guid userId)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var calendar = await _db.Calendars
                .Include(c => c.Events)
                .Include(c => c.Owner)
                .FirstOrDefaultAsync(c => c.OwnerId == userId);

            if (calendar == null)
                return Content("Tento uživatel nemá žádný kalendář.");

            return View("AdminCalendar", calendar);
        }

        [HttpPost("/admin/toggle-admin/{id}")]
        public async Task<IActionResult> ToggleAdmin(Guid id)
        {
            var check = RequireAdmin();
            if (check != null) return check;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
            if (user == null) return NotFound();

            user.IsAdmin = !user.IsAdmin;
            await _db.SaveChangesAsync();

            return Redirect("/admin");
        }
    }
}
