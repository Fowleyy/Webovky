using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;
using Semestralka.DTOs.Event;
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

            bool hasAccess =
                calendar.OwnerId == userId ||
                calendar.SharedWith.Any(s => s.UserId == userId);

            if (!hasAccess)
                return Unauthorized();

            var events = await _db.Events
                .Include(e => e.Category)
                .Where(e => e.CalendarId == calendarId)
                .Select(e => new
                {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartTime,
                    end = e.EndTime,
                    categoryId = e.CategoryId,
                    color = e.Category != null ? e.Category.Color : "#6b46c1"
                })
                .ToListAsync();

            return Ok(events);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateEventDto dto)
        {
            var userId = CurrentUserId;
            if (userId == null)
                return Unauthorized();

            var calendar = await _db.Calendars
                .Include(c => c.SharedWith)
                .FirstOrDefaultAsync(c => c.Id == dto.CalendarId);

            if (calendar == null)
                return NotFound();

            var permission = calendar.SharedWith
                .FirstOrDefault(s => s.UserId == userId)?.Permission;

            bool canEdit = calendar.OwnerId == userId || permission == "edit";

            if (!canEdit)
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

            return Ok(new { ev.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, UpdateEventDto dto)
        {
            var userId = CurrentUserId;
            if (userId == null)
                return Unauthorized();

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .ThenInclude(c => c.SharedWith)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            var permission = ev.Calendar.SharedWith
                .FirstOrDefault(s => s.UserId == userId)?.Permission;

            bool canEdit = ev.Calendar.OwnerId == userId || permission == "edit";

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

            return Ok(new { message = "Updated." });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userId = CurrentUserId;
            if (userId == null)
                return Unauthorized();

            var ev = await _db.Events
                .Include(e => e.Calendar)
                .ThenInclude(c => c.SharedWith)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            var permission = ev.Calendar.SharedWith
                .FirstOrDefault(s => s.UserId == userId)?.Permission;

            bool canEdit = ev.Calendar.OwnerId == userId || permission == "edit";

            if (!canEdit)
                return Unauthorized();

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted." });
        }
    }
}
