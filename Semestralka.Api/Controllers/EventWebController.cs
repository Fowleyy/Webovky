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

        private async Task<(bool canRead, bool canEdit)> CheckPermissions(Guid calendarId)
        {
            var calendar = await _db.Calendars
                .Include(c => c.SharedWith)
                .FirstOrDefaultAsync(c => c.Id == calendarId);

            if (calendar == null || UserId == null)
                return (false, false);

            if (calendar.OwnerId == UserId)
                return (true, true);

            var share = calendar.SharedWith.FirstOrDefault(s => s.UserId == UserId);

            if (share == null)
                return (false, false);

            if (share.Permission == "edit")
                return (true, true);

            return (true, false);
        }

        [HttpGet("create/{calendarId}")]
        public async Task<IActionResult> Create(Guid calendarId)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

            var calendar = await _db.Calendars
                .Include(c => c.SharedWith)
                .FirstOrDefaultAsync(c => c.Id == calendarId);

            if (calendar == null)
                return NotFound();

            bool hasAccess = false;

            if (isAdmin || calendar.OwnerId == UserId)
                hasAccess = true;
            else
            {
                var perm = calendar.SharedWith
                    .FirstOrDefault(s => s.UserId == UserId)?.Permission;

                if (perm == "edit")
                    hasAccess = true;
            }

            if (!hasAccess)
                return Unauthorized();

            ViewBag.CalendarId = calendarId;

            ViewBag.Categories = await _db.Categories.ToListAsync();

            return View();
        }

        [HttpPost("create/{calendarId}")]
        public async Task<IActionResult> CreatePost(Guid calendarId, Event model)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var calendar = await _db.Calendars
                .Include(c => c.SharedWith)
                .FirstOrDefaultAsync(c => c.Id == calendarId);

            if (calendar == null)
                return NotFound();

            var user = await _db.Users.FirstAsync(u => u.Id == UserId);

            bool canCreate =
                user.IsAdmin ||
                calendar.OwnerId == user.Id ||
                calendar.SharedWith.Any(s => s.UserId == user.Id && s.Permission == "edit");

            if (!canCreate)
                return Unauthorized();

            if (model.StartTime > model.EndTime)
            {
                ViewData["Error"] = "Začátek musí být před koncem.";
                ViewBag.CalendarId = calendarId;
                return View("Create", model);
            }

            model.Id = Guid.NewGuid();
            model.CalendarId = calendarId;

            _db.Events.Add(model);
            await _db.SaveChangesAsync();

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Událost vytvořena",
                Body = $"Událost „{model.Title}“ byla vytvořena.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                NotifyAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return RedirectToCalendar(model, user);
        }


        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            var user = await _db.Users.FirstAsync(u => u.Id == UserId);

            bool canEdit =
                user.IsAdmin ||
                ev.Calendar.OwnerId == UserId ||
                ev.Calendar.SharedWith.Any(s => s.UserId == UserId && s.Permission == "edit");

            if (!canEdit)
                return Unauthorized();

            return View(ev);
        }


        [HttpPost("edit/{id}")]
        public async Task<IActionResult> EditPost(Guid id, Event model)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .ThenInclude(c => c.SharedWith)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            var user = await _db.Users.FirstAsync(u => u.Id == UserId);

            bool canEdit =
                user.IsAdmin ||
                ev.Calendar.OwnerId == user.Id ||
                ev.Calendar.SharedWith.Any(s => s.UserId == user.Id && s.Permission == "edit");

            if (!canEdit)
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
            ev.CategoryId = model.CategoryId;

            await _db.SaveChangesAsync();

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Událost upravena",
                Body = $"Událost „{ev.Title}“ byla upravena.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                NotifyAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return RedirectToCalendar(ev, user);
        }


        [HttpGet("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .ThenInclude(c => c.SharedWith)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            var user = await _db.Users.FirstAsync(u => u.Id == UserId);

            bool canDelete =
                user.IsAdmin ||
                ev.Calendar.OwnerId == user.Id ||
                ev.Calendar.SharedWith.Any(s => s.UserId == user.Id && s.Permission == "edit");

            if (!canDelete)
                return Unauthorized();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Událost smazána",
                Body = $"Událost „{ev.Title}“ byla smazána.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                NotifyAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return RedirectToCalendar(ev, user);
        }




        [HttpGet("/events")]
        public async Task<IActionResult> List()
        {
            var login = RequireLogin();
            if (login != null) return login;

            var calendars = await _db.Calendars
                .Include(c => c.SharedWith)
                .Where(c =>
                       c.OwnerId == UserId ||
                       c.SharedWith.Any(s => s.UserId == UserId))
                .ToListAsync();

            var calendarIds = calendars.Select(c => c.Id);

            var events = await _db.Events
                .Where(e => calendarIds.Contains(e.CalendarId))
                .OrderBy(e => e.StartTime)
                .ToListAsync();

            return View("List", events);
        }
        private IActionResult RedirectToCalendar(Event ev, User currentUser)
        {
            var ownerId = ev.Calendar.OwnerId;
            var calendarId = ev.Calendar.Id;

            if (currentUser.IsAdmin && ownerId != currentUser.Id)
            {
                return Redirect($"/admin/calendar/{ownerId}");
            }

            if (ownerId != currentUser.Id)
            {
                return Redirect($"/calendar/{calendarId}");
            }

            return Redirect("/");
        }


    }
}
