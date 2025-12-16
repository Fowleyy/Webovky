using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Presentation.Controllers
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

        // GET /api/notifications/list
        [HttpGet("list")]
        public async Task<IActionResult> List()
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return Unauthorized();

            var userId = Guid.Parse(userIdStr);

            var notifications = await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();

            return Ok(notifications);
        }

        // POST /api/notifications/read?id=GUID
        [HttpPost("read")]
        public async Task<IActionResult> Read(Guid id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null)
                return NotFound();

            n.IsRead = true;
            await _db.SaveChangesAsync();

            return Ok();
        }

        // DELETE /api/notifications/delete/GUID
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var n = await _db.Notifications.FindAsync(id);
            if (n == null)
                return NotFound();

            _db.Notifications.Remove(n);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
