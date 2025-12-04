using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly CalendarDbContext _db;

        public NotificationsController(CalendarDbContext db)
        {
            _db = db;
        }

        private Guid? UserId =>
            HttpContext.Session.GetString("userid") is string id
                ? Guid.Parse(id)
                : (Guid?)null;

        // GET /api/notifications
        [HttpGet]
        public async Task<IActionResult> GetMy()
        {
            if (UserId == null)
                return Unauthorized();

            var items = await _db.Notifications
                .Where(n => n.UserId == UserId.Value)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(items);
        }

        // POST /api/notifications/read/{id}
        [HttpPost("read/{id}")]
        public async Task<IActionResult> MarkRead(Guid id)
        {
            if (UserId == null)
                return Unauthorized();

            var notif = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == UserId.Value);

            if (notif == null)
                return NotFound();

            notif.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok();
        }

        // DELETE /api/notifications/delete/{id}
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (UserId == null)
                return Unauthorized();

            var notif = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == id && n.UserId == UserId.Value);

            if (notif != null)
            {
                _db.Notifications.Remove(notif);
                await _db.SaveChangesAsync();
            }

            return Ok();
        }

        // TEST NOTIFICATION
        [HttpPost("test")]
        public async Task<IActionResult> CreateTest()
        {
            if (UserId == null)
                return Unauthorized();

            var notif = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = UserId.Value,
                Title = "Test notifikace",
                Body = "Toto je testovací notifikace.",
                CreatedAt = DateTime.UtcNow,
                NotifyAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notif);
            await _db.SaveChangesAsync();

            return Ok(notif);
        }
    }
}
