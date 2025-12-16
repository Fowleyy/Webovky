using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Infrastructure.Services
{
    public class CalendarService
    {
        private readonly CalendarDbContext _db;

        public CalendarService(CalendarDbContext db)
        {
            _db = db;
        }

        public async Task<Calendar> GetUserCalendarAsync(Guid userId)
        {
            var calendar = await _db.Calendars
                .Include(c => c.Events)
                .FirstOrDefaultAsync(c => c.OwnerId == userId);

            if (calendar == null)
                throw new Exception("Kalendář uživatele nebyl nalezen");

            return calendar;
        }

        public async Task<Calendar> CreateIfNotExistsAsync(Guid userId)
        {
            var calendar = await _db.Calendars
                .FirstOrDefaultAsync(c => c.OwnerId == userId);

            if (calendar != null)
                return calendar;

            calendar = new Calendar
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                Color = "#1976d2",
                Visibility = "private"
            };

            _db.Calendars.Add(calendar);
            await _db.SaveChangesAsync();

            return calendar;
        }


        public async Task UpdateColorAsync(Guid userId, string color)
        {
            var calendar = await _db.Calendars
                .FirstOrDefaultAsync(c => c.OwnerId == userId);

            if (calendar == null)
                throw new Exception("Kalendář nenalezen");

            calendar.Color = color;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateVisibilityAsync(Guid userId, string visibility)
        {
            var calendar = await _db.Calendars
                .FirstOrDefaultAsync(c => c.OwnerId == userId);

            if (calendar == null)
                throw new Exception("Kalendář nenalezen");

            calendar.Visibility = visibility;
            await _db.SaveChangesAsync();
        }
    }
}
