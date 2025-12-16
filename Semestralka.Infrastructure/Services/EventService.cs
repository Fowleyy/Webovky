using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Infrastructure.Services
{
    public class EventService
    {
        private readonly CalendarDbContext _db;

        public EventService(CalendarDbContext db)
        {
            _db = db;
        }

        public async Task<List<Event>> GetUserEventsAsync(Guid userId)
        {
            return await _db.Events
                .Include(e => e.Calendar)
                .Where(e => e.Calendar.UserId == userId)
                .OrderBy(e => e.Start)
                .ToListAsync();
        }

        public async Task CreateAsync(Event ev, Guid userId)
        {
            var calendar = await _db.Calendars
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (calendar == null)
                throw new Exception("Kalendář nenalezen");

            ev.Id = Guid.NewGuid();
            ev.CalendarId = calendar.Id;

            _db.Events.Add(ev);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(Guid eventId, Guid userId)
        {
            var ev = await _db.Events
                .Include(e => e.Calendar)
                .FirstOrDefaultAsync(e => e.Id == eventId);

            if (ev == null)
                throw new Exception("Událost neexistuje");

            if (ev.Calendar.UserId != userId)
                throw new Exception("Nemáš právo tuto událost smazat");

            _db.Events.Remove(ev);
            await _db.SaveChangesAsync();
        }
    }
}
