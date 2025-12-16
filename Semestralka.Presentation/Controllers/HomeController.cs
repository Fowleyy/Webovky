using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;
using Semestralka.Presentation.Models;

namespace Semestralka.Presentation.Controllers
{
    public class HomeController : Controller
    {
        private readonly CalendarDbContext _db;
        private readonly ILogger<HomeController> _logger;

        public HomeController(CalendarDbContext db, ILogger<HomeController> logger)
        {
            _db = db;
            _logger = logger;
        }

        // =========================
        // HLAVNÍ KALENDÁŘ (OWNER)
        // =========================
        public async Task<IActionResult> Index()
        {
            var uid = HttpContext.Session.GetString("userid");
            if (uid == null)
                return Redirect("/login");

            var userId = Guid.Parse(uid);

            var calendar = await _db.Calendars
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.OwnerId == userId);

            if (calendar == null)
            {
                calendar = new Calendar
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userId,
                    Color = "#4287f5",
                    Visibility = "private"
                };

                _db.Calendars.Add(calendar);
                await _db.SaveChangesAsync();
            }

            HttpContext.Session.SetString(
                "currentCalendarId",
                calendar.Id.ToString()
            );

            return View("FullCalendarHome", calendar);
        }

        // =========================
        // SDÍLENÉ KALENDÁŘE
        // =========================
        [HttpGet("/shared-calendars")]
        public async Task<IActionResult> SharedCalendars()
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Index");

            var userId = Guid.Parse(userIdStr);

            var shared = await _db.CalendarShares
                .Include(cs => cs.Calendar)
                    .ThenInclude(c => c.Owner)
                .Where(cs => cs.UserId == userId)
                .ToListAsync();

            return View(shared);
        }

        // =========================
        // ZOBRAZENÍ SDÍLENÉHO KALENDÁŘE
        // =========================
        [HttpGet("/calendar/{id}")]
        public async Task<IActionResult> ViewShared(Guid id)
        {
            var uid = HttpContext.Session.GetString("userid");
            if (uid == null)
                return Redirect("/login");

            var userId = Guid.Parse(uid);

            var calendar = await _db.Calendars
                .Include(c => c.Owner)
                .Include(c => c.SharedWith)
                    .ThenInclude(s => s.User)
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    (c.OwnerId == userId ||
                     c.SharedWith.Any(s => s.UserId == userId))
                );

            if (calendar == null)
                return Unauthorized();

            HttpContext.Session.SetString(
                "currentCalendarId",
                calendar.Id.ToString()
            );

            return View("FullCalendarHome", calendar);
        }

        // =========================
        // OSTATNÍ
        // =========================
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id
            });
        }

        public IActionResult Landing()
        {
            if (HttpContext.Session.GetString("userid") != null)
                return RedirectToAction("Index");

            return View();
        }
    }
}
