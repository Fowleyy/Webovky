using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;


namespace Semestralka.Services
{
    public class NotificationService : BackgroundService
    {
        private readonly IServiceProvider _provider;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IServiceProvider provider, ILogger<NotificationService> logger)
        {
            _provider = provider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("NotificationService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await ProcessNotifications(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in NotificationService");
                }

                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
        }

        private async Task ProcessNotifications(CancellationToken token)
        {
            using var scope = _provider.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();

            var now = DateTime.UtcNow;
            var soon = now.AddMinutes(10); // upozornění 10 minut před událostí

            var events = await db.Events
                .Include(e => e.Calendar)
                .Where(e =>
                    e.StartTime >= now &&
                    e.StartTime <= soon)
                .ToListAsync(token);

            foreach (var ev in events)
            {
                bool exists = await db.Notifications.AnyAsync(n =>
                    n.EventId == ev.Id && n.UserId == ev.Calendar.OwnerId,
                    token);

                if (exists) continue;

                var notifyAt = ev.StartTime.UtcDateTime;  // ✔ OPRAVA DateTimeOffset → DateTime

                var n = new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = ev.Calendar.OwnerId,
                    EventId = ev.Id,
                    Title = $"Blíží se událost: {ev.Title}",
                    Body = $"Událost začne v {ev.StartTime.LocalDateTime:t}.",
                    NotifyAt = notifyAt,
                    CreatedAt = DateTime.UtcNow
                };

                db.Notifications.Add(n);
            }

            await db.SaveChangesAsync(token);
        }
    }
}
