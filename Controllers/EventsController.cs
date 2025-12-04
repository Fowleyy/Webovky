using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.DTOs;
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

        // ============================================================
        // 🟢 GET EVENTS (now supports SESSION + JWT)
        // GET /api/events?calendarId=xxx
        // ============================================================
        [HttpGet]
        [AllowAnonymous] // authentication handled manually below
        public async Task<IActionResult> GetEvents([FromQuery] Guid calendarId)
        {
            Guid currentUserId;

            // 🔥 1) Try SESSION auth first (MVC UI)
            var sessionUser = HttpContext.Session.GetString("userid");
            if (sessionUser != null)
            {
                currentUserId = Guid.Parse(sessionUser);
            }
            else
            {
                // 🔥 2) If no session, try JWT auth (API)
                if (!User.Identity?.IsAuthenticated ?? true)
                    return Unauthorized(new { message = "Unauthorized" });

                currentUserId = Guid.Parse(User.FindFirst("userid")!.Value);
            }

            // 🔥 3) Verify the calendar belongs to this user
            var owns = await _db.Calendars.AnyAsync(c => c.Id == calendarId && c.OwnerId == currentUserId);
            if (!owns)
                return Unauthorized(new { message = "You do not own this calendar." });

            // 🔥 4) Return events
            var events = await _db.Events
                .Where(e => e.CalendarId == calendarId)
                .Select(e => new {
                    id = e.Id,
                    title = e.Title,
                    start = e.StartTime,
                    end = e.EndTime
                })
                .ToListAsync();

            return Ok(events);
        }



        // ============================================================
        // CREATE EVENT
        // ============================================================
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(EventCreateDto dto)
        {
            var userId = Guid.Parse(User.FindFirst("userid")!.Value);

            var owns = await _db.Calendars.AnyAsync(c => c.Id == dto.CalendarId && c.OwnerId == userId);
            if (!owns)
                return Unauthorized(new { message = "You do not own this calendar." });

            var ev = new Event
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

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();

            return Ok(new { ev.Id });
        }



        // ============================================================
        // UPDATE EVENT
        // ============================================================
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> Update(Guid id, EventUpdateDto dto)
        {
            var ev = await _db.Events.Include(e => e.Calendar).FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            var userId = Guid.Parse(User.FindFirst("userid")!.Value);
            if (ev.Calendar.OwnerId != userId)
                return Unauthorized(new { message = "You do not own this calendar." });

            ev.Title = dto.Title;
            ev.Description = dto.Description;
            ev.StartTime = dto.StartTime;
            ev.EndTime = dto.EndTime;
            ev.Location = dto.Location;
            ev.IsAllDay = dto.IsAllDay;

            await _db.SaveChangesAsync();

            return Ok(new { message = "Updated." });
        }



        // ============================================================
        // DELETE EVENT
        // ============================================================
        [HttpDelete("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> Delete(Guid id)
        {
            Guid currentUserId;

            // 1) SESSION AUTH (MVC UI)
            var sessionUser = HttpContext.Session.GetString("userid");
            if (sessionUser != null)
            {
                currentUserId = Guid.Parse(sessionUser);
            }
            else
            {
                // 2) JWT AUTH (API client)
                if (!User.Identity?.IsAuthenticated ?? true)
                    return Unauthorized();

                currentUserId = Guid.Parse(User.FindFirst("userid")!.Value);
            }

            // 3) Najdi událost
            var ev = await _db.Events
                .Include(e => e.Calendar)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (ev == null)
                return NotFound();

            // 4) Ověření vlastnictví
            if (ev.Calendar.OwnerId != currentUserId)
                return Unauthorized();

            // 5) Smazání
            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted." });
        }

    }
}
