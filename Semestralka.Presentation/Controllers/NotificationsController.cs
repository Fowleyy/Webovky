using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Presentation.Controllers;

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