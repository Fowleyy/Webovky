using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Infrastructure.Services
{
    public class NotificationService
    {
        private readonly CalendarDbContext _db;

        public NotificationService(CalendarDbContext db)
        {
            _db = db;
        }

        public async Task<List<Notification>> GetUserNotificationsAsync(Guid userId)
        {
            return await _db.Notifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task CreateAsync(
            Guid userId,
            string title,
            string body,
            Guid? eventId,
            DateTime notifyAt)
        {
            var notification = new Notification
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                Title = title,
                Body = body,
                EventId = eventId,
                NotifyAt = notifyAt,
                IsRead = false,
                CreatedAt = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                throw new Exception("Notifikace nenalezena");

            notification.IsRead = true;
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid notificationId, Guid userId)
        {
            var notification = await _db.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                throw new Exception("Notifikace nenalezena");

            _db.Notifications.Remove(notification);
            await _db.SaveChangesAsync();
        }

    }
}
