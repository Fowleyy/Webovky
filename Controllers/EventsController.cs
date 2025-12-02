using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.DTOs.Event;
using Semestralka.Models;
using System.Security.Claims;

namespace Semestralka.Controllers
{
    [ApiController]
    [Route("api/events")]
    [Authorize]
    public class EventsController : ControllerBase
    {
        private readonly CalendarDbContext _db;

        public EventsController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: api/events?calendarId=xxxx
        [HttpGet]
        public async Task<IActionResult> GetEvents([FromQuery] Guid calendarId)
        {
            var userId = GetUserId();

            // ověř vlastnictví kalendáře
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == calendarId && c.OwnerId == userId);
            if (calendar == null)
                return Unauthorized(new { message = "You do not own this calendar" });

            var events = await _db.Events
                .Where(e => e.CalendarId == calendarId)
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvent(Guid id)
        {
            var userId = GetUserId();

            var e = await _db.Events
                .Include(x => x.Participants)
                .Include(x => x.Calendar)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null)
                return NotFound();

            if (e.Calendar.OwnerId != userId)
                return Unauthorized(new { message = "Not your event" });

            return Ok(e);
        }

        // POST: api/events
        [HttpPost]
        public async Task<IActionResult> CreateEvent(CreateEventDto dto)
        {
            var userId = GetUserId();

            // ověř vlastnictví kalendáře
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == dto.CalendarId && c.OwnerId == userId);
            if (calendar == null)
                return Unauthorized(new { message = "You do not own this calendar" });

            var e = new Event
            {
                Id = Guid.NewGuid(),
                CalendarId = dto.CalendarId,
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                Location = dto.Location,
                IsAllDay = dto.IsAllDay
            };

            _db.Events.Add(e);
            await _db.SaveChangesAsync();

            return Ok(e);
        }

        // PUT: api/events/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEvent(Guid id, UpdateEventDto dto)
        {
            var userId = GetUserId();

            var e = await _db.Events
                .Include(x => x.Calendar)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null)
                return NotFound();

            if (e.Calendar.OwnerId != userId)
                return Unauthorized(new { message = "Not your event" });

            e.Title = dto.Title;
            e.Description = dto.Description;
            e.StartTime = dto.StartTime;
            e.EndTime = dto.EndTime;
            e.Location = dto.Location;
            e.IsAllDay = dto.IsAllDay;

            await _db.SaveChangesAsync();

            return Ok(e);
        }

        // DELETE: api/events/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvent(Guid id)
        {
            var userId = GetUserId();

            var e = await _db.Events
                .Include(x => x.Calendar)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (e == null)
                return NotFound();

            if (e.Calendar.OwnerId != userId)
                return Unauthorized(new { message = "Not your event" });

            _db.Events.Remove(e);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }
    }
}
