using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    [ApiController]
    [Route("api/events")]
    public class EventsController : ControllerBase
    {
        private readonly CalendarDbContext _db;

        public EventsController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid? CurrentUserId
        {
            get
            {
                var sessionId = HttpContext.Session.GetString("userid");
                if (sessionId != null) return Guid.Parse(sessionId);

                var claim = User.FindFirst("userid")?.Value;
                if (claim != null) return Guid.Parse(claim);

                return null;
            }
        }

        private async Task<User?> GetCurrentUser()
        {
            var id = CurrentUserId;
            if (id == null) return null;
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        [HttpGet]
        public async Task<IActionResult> GetEvents(Guid calendarId)
        {
            var userId = CurrentUserId;
            if (userId == null)
                return Unauthorized();

            var calendar = await _db.Calendars
                .Include(c => c.SharedWith)
                .FirstOrDefaultAsync(c => c.Id == calendarId);

            if (calendar == null)
                return NotFound();

            var user = await GetCurrentUser();
            bool hasAccess =
                (user?.IsAdmin ?? false) ||
                calendar.OwnerId == userId ||
                calendar.SharedWith.Any(s => s.UserId == userId);

            if (!hasAccess)
                return Unauthorized();

            var events = await _db.Events
                .Where(e => e.CalendarId == calendarId)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartTime,
                    end = e.EndTime,
                    color = e.Category != null ? e.Category.ColorHex : "#6b46c1"
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Event dto)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();
            
            var calendar = await _db.Calendars
                .Include(c => c.SharedWith)
                .FirstOrDefaultAsync(c => c.Id == dto.CalendarId);

            if (calendar == null)
                return NotFound();

            bool canCreate =
                user.IsAdmin ||
                calendar.OwnerId == user.Id ||
                calendar.SharedWith.Any(s => s.UserId == user.Id && s.Permission == "edit");

            if (!canCreate)
                return Unauthorized();

            var ev = new Event
            {
                Id = Guid.NewGuid(),
                CalendarId = dto.CalendarId,
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Location = dto.Location,
                IsAllDay = dto.IsAllDay,
                CategoryId = dto.CategoryId
            };

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            // 🔔 Notification – Created
            _db.Notifications.Add(new Notification
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Title = "Událost vytvořena",
                Body = $"Událost „{ev.Title}“ byla vytvořena.",
                IsRead = false,
                CreatedAt = DateTime.UtcNow,
                NotifyAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();

            return Ok(new { ev.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, Event dto)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .ThenInclude(c => c.SharedWith)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            bool canEdit =
                user.IsAdmin ||
                ev.Calendar.OwnerId == user.Id ||
                ev.Calendar.SharedWith.Any(s => s.UserId == user.Id && s.Permission == "edit");

            if (!canEdit)
                return Unauthorized();

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.StartTime = dto.StartTime;
            ev.EndTime = dto.EndTime;
            ev.Location = dto.Location;
            ev.IsAllDay = dto.IsAllDay;
            ev.CategoryId = dto.CategoryId;

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

            return Ok(new { message = "Updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var user = await GetCurrentUser();
            if (user == null) return Unauthorized();

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .ThenInclude(c => c.SharedWith)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            bool canDelete =
                user.IsAdmin ||
                ev.Calendar.OwnerId == user.Id ||
                ev.Calendar.SharedWith.Any(s => s.UserId == user.Id && s.Permission == "edit");

            if (!canDelete)
                return Unauthorized();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            // 🔔 Notification – Deleted
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

            return Ok(new { message = "Deleted." });
        }
    }
}
