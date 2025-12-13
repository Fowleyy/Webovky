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

        public async Task<IActionResult> Index()
        {
            var http = HttpContext;
            if (http == null)
                return Redirect("/login");

            var uid = http.Session.GetString("userid");
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

            http.Session.SetString("currentCalendarId", calendar.Id.ToString());
            return View("FullCalendarHome", calendar);
        }

        [HttpGet("/shared-calendars")]
        public async Task<IActionResult> SharedCalendars()
        {
            var http = HttpContext;
            if (http == null)
                return Redirect("/login");

            var uid = http.Session.GetString("userid");
            if (uid == null)
                return Redirect("/login");

            var userId = Guid.Parse(uid);

            var shared = await _db.CalendarShares
                .Include(s => s.Calendar)
                    .ThenInclude(c => c.Owner)
                .Where(s => s.UserId == userId)
                .ToListAsync();

            return View("SharedCalendars", shared);
        }

        [HttpGet("/calendar/{id}")]
        public async Task<IActionResult> ViewShared(Guid id)
        {
            var http = HttpContext;
            if (http == null)
                return Redirect("/login");

            var uid = http.Session.GetString("userid");
            if (uid == null)
                return Redirect("/login");

            var userId = Guid.Parse(uid);

            var result = await _db.Calendars
                .Include(c => c.Owner)
                .Include(c => c.SharedWith)
                    .ThenInclude(s => s.User)
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c =>
                    c.Id == id &&
                    (c.OwnerId == userId || c.SharedWith.Any(s => s.UserId == userId))
                );

            if (result == null)
                return Unauthorized();

            http.Session.SetString("currentCalendarId", result.Id.ToString());
            return View("FullCalendarHome", result);
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

        public IActionResult Landing()
        {
            if (HttpContext?.Session.GetString("userid") != null)
                return RedirectToAction("Index");

            return View();
        }
    }
}
