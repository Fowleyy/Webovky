using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
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

        // HLAVNÍ STRÁNKA – osobní kalendář
        public async Task<IActionResult> Index()
        {
            // Pokud není přihlášen → login
            var uid = HttpContext.Session.GetString("userid");
            if (uid == null)
                return Redirect("/login");

            var userId = Guid.Parse(uid);

            // Najdi hlavní kalendář uživatele
            var calendar = await _db.Calendars
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.OwnerId == userId);

            // Vytvoř hlavní kalendář pokud neexistuje
            if (calendar == null)
            {
                calendar = new Calendar
                {
                    Id = Guid.NewGuid(),
                    OwnerId = userId,
                    Title = "Můj kalendář",
                    Color = "#4287f5",
                    Visibility = "private"
                };

                _db.Calendars.Add(calendar);
                await _db.SaveChangesAsync();
            }

            // Nastavíme currentCalendarId do Session pro sdílení
            HttpContext.Session.SetString("currentCalendarId", calendar.Id.ToString());

            return View("FullCalendarHome", calendar);
        }


        // 🟣 ZOBRAZENÍ SDÍLENÝCH KALENDÁŘŮ
        [HttpGet("/shared-calendars")]
        public async Task<IActionResult> SharedCalendars()
        {
            var uid = HttpContext.Session.GetString("userid");
            if (uid == null)
                return Redirect("/login");

            var userId = Guid.Parse(uid);

            var shared = await _db.CalendarShares
                .Include(s => s.Calendar)
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View("SharedCalendars", shared);
        }


        // 🟣 ZOBRAZENÍ KONKRÉTNÍHO SDÍLENÉHO KALENDÁŘE
        [HttpGet("/calendar/{id}")]
        public async Task<IActionResult> ViewShared(Guid id)
        {
            var uid = HttpContext.Session.GetString("userid");
            if (uid == null)
                return Redirect("/login");

            var userId = Guid.Parse(uid);

            var calendar = await _db.Calendars
                .Include(c => c.Events)
                .Include(c => c.SharedWith)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    (c.OwnerId == userId || c.SharedWith.Any(s => s.UserId == userId))
                );

            if (calendar == null)
                return Unauthorized();

            // nastavíme currentCalendarId
            HttpContext.Session.SetString("currentCalendarId", calendar.Id.ToString());

            return View("FullCalendarHome", calendar);
        }


        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}
