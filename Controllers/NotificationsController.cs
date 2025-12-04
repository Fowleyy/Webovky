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
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly CalendarDbContext _db;
        private Guid UserId => Guid.Parse(User.FindFirst("userid")!.Value);

        public NotificationsController(CalendarDbContext db)
        {
            _db = db;
        }

        // GET notifications for logged user
        [HttpGet]
        public async Task<IActionResult> GetMyNotifications()
        {
            var list = await _db.Notifications
                .Where(n => n.UserId == UserId)
                .Select(n => new NotificationListDto
                {
                    Id = n.Id,
                    EventId = n.EventId!.Value,
                    SendTime = n.SendTime,
                    IsSent = n.IsSent
                })
                .ToListAsync();

            return Ok(list);
        }

        // CREATE notification
        [HttpPost]
        public async Task<IActionResult> Create(NotificationCreateDto dto)
        {
            var ev = await _db.Events.Include(e => e.Calendar)
                .FirstOrDefaultAsync(e => e.Id == dto.EventId);

            if (ev == null)
                return NotFound(new { message = "Event not found" });

            if (ev.Calendar.OwnerId != UserId)
                return Unauthorized(new { message = "Not your event" });

            var noti = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = UserId,
                EventId = dto.EventId,
                SendTime = dto.SendTime,
                Type = "Internal",
                IsSent = false
            };

            _db.Notifications.Add(noti);
            await _db.SaveChangesAsync();

            return Ok(new { noti.Id });
        }

        // DELETE notification
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var noti = await _db.Notifications.FirstOrDefaultAsync(n => n.Id == id);
            if (noti == null)
                return NotFound();

            if (noti.UserId != UserId)
                return Unauthorized();

            _db.Notifications.Remove(noti);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }
    }
}
