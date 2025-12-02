using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.DTOs.Calendar;
using Semestralka.Models;
using System.Security.Claims;

namespace Semestralka.Controllers
{
    [ApiController]
    [Route("api/calendars")]
    [Authorize]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarDbContext _db;

        public CalendarController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        }

        // GET: api/calendars
        [HttpGet]
        public async Task<IActionResult> GetUserCalendars()
        {
            var userId = GetUserId();

            var calendars = await _db.Calendars
                .Where(c => c.OwnerId == userId)
                .ToListAsync();

            return Ok(calendars);
        }

        // POST: api/calendars
        [HttpPost]
        public async Task<IActionResult> CreateCalendar(CreateCalendarDto dto)
        {
            var userId = GetUserId();

            var calendar = new Calendar
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                Title = dto.Title,
                Color = dto.Color,
                Visibility = dto.Visibility
            };

            _db.Calendars.Add(calendar);
            await _db.SaveChangesAsync();

            return Ok(calendar);
        }

        // GET: api/calendars/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCalendar(Guid id)
        {
            var userId = GetUserId();
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (calendar == null)
                return NotFound();

            return Ok(calendar);
        }

        // PUT: api/calendars/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCalendar(Guid id, UpdateCalendarDto dto)
        {
            var userId = GetUserId();
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (calendar == null)
                return NotFound();

            calendar.Title = dto.Title;
            calendar.Color = dto.Color;
            calendar.Visibility = dto.Visibility;

            await _db.SaveChangesAsync();

            return Ok(calendar);
        }

        // DELETE: api/calendars/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCalendar(Guid id)
        {
            var userId = GetUserId();
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == id && c.OwnerId == userId);

            if (calendar == null)
                return NotFound();

            _db.Calendars.Remove(calendar);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }
    }
}
