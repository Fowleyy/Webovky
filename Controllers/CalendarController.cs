using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.DTOs;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/calendars")]
    public class CalendarsController : ControllerBase
    {
        private readonly CalendarDbContext _db;

        public CalendarsController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid UserId => Guid.Parse(User.FindFirst("userid")!.Value);

        // GET all calendars for logged user
        [HttpGet]
        public async Task<IActionResult> GetMyCalendars()
        {
            var calendars = await _db.Calendars
                .Where(c => c.OwnerId == UserId)
                .Select(c => new {
                    c.Id,
                    c.Title,
                    c.Color,
                    c.Visibility
                })
                .ToListAsync();

            return Ok(calendars);
        }

        // CREATE
        [HttpPost]
        public async Task<IActionResult> Create(CalendarCreateDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Title))
                return BadRequest(new { message = "Calendar title is required." });

            var calendar = new Calendar
            {
                Id = Guid.NewGuid(),
                OwnerId = UserId,
                Title = dto.Title,
                Color = dto.Color,
                Visibility = dto.Visibility
            };

            _db.Calendars.Add(calendar);
            await _db.SaveChangesAsync();

            return Ok(new { calendar.Id });
        }

        // UPDATE
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CalendarUpdateDto dto)
        {
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == id);
            if (calendar == null)
                return NotFound();

            if (calendar.OwnerId != UserId)
                return Unauthorized(new { message = "This calendar does not belong to you." });

            calendar.Title = dto.Title;
            calendar.Color = dto.Color;
            calendar.Visibility = dto.Visibility;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Updated." });
        }

        // DELETE
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == id);
            if (calendar == null)
                return NotFound();

            if (calendar.OwnerId != UserId)
                return Unauthorized(new { message = "This calendar does not belong to you." });

            _db.Calendars.Remove(calendar);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted." });
        }
    }
}
