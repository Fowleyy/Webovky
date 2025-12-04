using Microsoft.EntityFrameworkCore;
using Semestralka.Data;

namespace Semestralka.Services
{
    public class NotificationWorker : BackgroundService
    {
        private readonly IServiceProvider _provider;

        public NotificationWorker(IServiceProvider provider)
        {
            _provider = provider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // běží každou minutu
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _provider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<CalendarDbContext>();

                var now = DateTimeOffset.UtcNow;

                var due = await db.Notifications
                    .Where(n => !n.IsSent && n.SendTime <= now)
                    .Include(n => n.Event)
                    .ToListAsync(stoppingToken);

                foreach (var n in due)
                {
                    Console.WriteLine($"[NOTIFICATION] Event '{n.Event!.Title}' pro uživatele {n.UserId} právě teď!");
                    n.IsSent = true;
                }

                await db.SaveChangesAsync(stoppingToken);

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }
    }
}
