using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    [Route("calendar/share")]
    public class ShareController : Controller
    {
        private readonly CalendarDbContext _db;

        public ShareController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid? UserId =>
            HttpContext.Session.GetString("userid") is string id
                ? Guid.Parse(id)
                : (Guid?)null;


        // GET /calendar/share/{calendarId}
        [HttpGet("{calendarId}")]
        public async Task<IActionResult> Index(Guid calendarId)
        {
            if (UserId == null) return Redirect("/login");

            var cal = await _db.Calendars
                .Include(c => c.SharedWith).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(c => c.Id == calendarId && c.OwnerId == UserId);

            if (cal == null)
                return Unauthorized();

            return View(cal);
        }


        // POST /calendar/share/{calendarId}
        [HttpPost("{calendarId}")]
        public async Task<IActionResult> Share(Guid calendarId, string email, string permission)
        {
            if (UserId == null) return Redirect("/login");

            var calendar = await _db.Calendars
                .FirstOrDefaultAsync(c => c.Id == calendarId && c.OwnerId == UserId);

            if (calendar == null) return Unauthorized();

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());

            if (user == null)
            {
                TempData["Error"] = "Uživatel s tímto emailem neexistuje.";
                return Redirect($"/calendar/share/{calendarId}");
            }

            if (user.Id == UserId)
            {
                TempData["Error"] = "Nemůžeš sdílet kalendář sám sobě.";
                return Redirect($"/calendar/share/{calendarId}");
            }

            // už sdílíš?
            var existing = await _db.CalendarShares
                .FirstOrDefaultAsync(s => s.CalendarId == calendarId && s.UserId == user.Id);

            if (existing == null)
            {
                var record = new CalendarShare
                {
                    Id = Guid.NewGuid(),
                    CalendarId = calendarId,
                    UserId = user.Id,
                    Permission = permission
                };

                _db.CalendarShares.Add(record);
            }
            else
            {
                existing.Permission = permission; // update práv
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Kalendář byl úspěšně sdílen.";
            return Redirect($"/calendar/share/{calendarId}");
        }


        // DELETE /calendar/share/remove/{id}
        [HttpGet("remove/{id}")]
        public async Task<IActionResult> Remove(Guid id)
        {
            if (UserId == null) return Redirect("/login");

            var entry = await _db.CalendarShares
                .Include(s => s.Calendar)
                .FirstOrDefaultAsync(s => s.Id == id && s.Calendar.OwnerId == UserId);

            if (entry != null)
            {
                _db.CalendarShares.Remove(entry);
                await _db.SaveChangesAsync();
            }

            return Redirect($"/calendar/share/{entry.CalendarId}");
        }
    }
}
