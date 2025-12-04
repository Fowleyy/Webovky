using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    [Route("event")]
    public class EventWebController : Controller
    {
        private readonly CalendarDbContext _db;

        public EventWebController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid? UserId =>
            HttpContext.Session.GetString("userid") is string id
                ? Guid.Parse(id)
                : null;

        private IActionResult? RequireLogin()
        {
            if (UserId == null)
                return Redirect("/login");

            return null;
        }

        // ============================================================
        // GET /event/create/{calendarId}
        // Otevření formuláře pro vytvoření události
        // ============================================================
        [HttpGet("create/{calendarId}")]
        public async Task<IActionResult> Create(Guid calendarId)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null || calendar.OwnerId != UserId)
                return Unauthorized();

            ViewBag.CalendarId = calendarId;
            return View();
        }

        // ============================================================
        // POST /event/create/{calendarId}
        // Vytvoření události
        // ============================================================
        [HttpPost("create/{calendarId}")]
        public async Task<IActionResult> CreatePost(Guid calendarId, Event model)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId);
            if (calendar == null || calendar.OwnerId != UserId)
                return Unauthorized();

            if (model.StartTime > model.EndTime)
            {
                ViewData["Error"] = "Začátek události musí být před koncem.";
                ViewBag.CalendarId = calendarId;
                return View("Create", model);
            }

            model.Id = Guid.NewGuid();
            model.CalendarId = calendarId;

            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            // Nová logika → vracíme uživatele na hlavní stránku
            return Redirect("/");
        }

        // ============================================================
        // GET /event/edit/{id}
        // Formulář pro úpravu události
        // ============================================================
        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null || ev.Calendar.OwnerId != UserId)
                return Unauthorized();

            return View(ev);
        }

        // ============================================================
        // POST /event/edit/{id}
        // Uložení úprav události
        // ============================================================
        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditPost(Guid id, Event model)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null || ev.Calendar.OwnerId != UserId)
                return Unauthorized();

            if (model.StartTime > model.EndTime)
            {
                ViewData["Error"] = "Začátek musí být před koncem.";
                return View("Edit", model);
            }

            ev.Title = model.Title;
            ev.Description = model.Description;
            ev.StartTime = model.StartTime;
            ev.EndTime = model.EndTime;
            ev.Location = model.Location;
            ev.IsAllDay = model.IsAllDay;

            await _db.SaveChangesAsync();

            // Vrací na hlavní kalendář
            return Redirect("/");
        }

        // ============================================================
        // GET /event/delete/{id}
        // Smazání události
        // ============================================================
        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null || ev.Calendar.OwnerId != UserId)
                return Unauthorized();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            // Vrací na hlavní stránku
            return Redirect("/");
        }
        // ============================================================
        // GET /events
        // Stránka se seznamem událostí
        // ============================================================
        [HttpGet("/events")]
        public async Task<IActionResult> List()
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            // najdeme všechny kalendáře uživatele
            var calendars = await _db.Calendars
                .Where(c => c.OwnerId == UserId)
                .ToListAsync();

            if (calendars.Count == 0)
                return View(new List<Event>());

            var calendarIds = calendars.Select(c => c.Id);

            // načteme všechny události uživatele
            var events = await _db.Events
                .Where(e => calendarIds.Contains(e.CalendarId))
                .OrderBy(e => e.StartTime)
                .ToListAsync();

            return View("List", events);
        }

    }
}
