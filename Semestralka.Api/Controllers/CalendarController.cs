using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.DTOs;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    [ApiController]
    [Route("api/calendars")]
    public class CalendarController : ControllerBase
    {
        private readonly CalendarDbContext _db;

        public CalendarController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid? UserId => HttpContext.Session.GetString("userid") is string s ? Guid.Parse(s) : (Guid?)null;

        [HttpPost]
        public async Task<IActionResult> Create(CalendarCreateDto dto)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();

            var calendar = new Calendar
            {
                Id = Guid.NewGuid(),
                OwnerId = uid.Value,
                Color = dto.Color,
                Visibility = dto.Visibility
            };

            _db.Calendars.Add(calendar);
            await _db.SaveChangesAsync();

            return Ok(new { calendar.Id });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(Guid id, CalendarUpdateDto dto)
        {
            var uid = UserId;
            if (uid == null) return Unauthorized();

            var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == id);
            if (calendar == null) return NotFound();
            if (calendar.OwnerId != uid) return Unauthorized();

            calendar.Color = dto.Color;
            calendar.Visibility = dto.Visibility;

            await _db.SaveChangesAsync();
            return Ok(new { message = "Updated." });
        }
    }
}
