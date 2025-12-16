using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Infrastructure.Services
{
    public class ShareService
    {
        private readonly CalendarDbContext _db;

        public ShareService(CalendarDbContext db)
        {
            _db = db;
        }

        public async Task ShareCalendarAsync(
            Guid ownerId,
            Guid targetUserId,
            string permission)
        {
            var calendar = await _db.Calendars
                .FirstOrDefaultAsync(c => c.OwnerId == ownerId);

            if (calendar == null)
                throw new Exception("Kalendář nenalezen");

            var exists = await _db.CalendarShares.AnyAsync(s =>
                s.CalendarId == calendar.Id &&
                s.UserId == targetUserId);

            if (exists)
                throw new Exception("Kalendář je již sdílen");

            var share = new CalendarShare
            {
                Id = Guid.NewGuid(),
                CalendarId = calendar.Id,
                UserId = targetUserId,
                Permission = permission
            };

            _db.CalendarShares.Add(share);
            await _db.SaveChangesAsync();
        }

        public async Task<List<CalendarShare>> GetSharedCalendarsAsync(Guid userId)
        {
            return await _db.CalendarShares
                .Include(s => s.Calendar)
                .Where(s => s.UserId == userId)
                .ToListAsync();
        }

        public async Task RemoveShareAsync(Guid shareId, Guid ownerId)
        {
            var share = await _db.CalendarShares
                .Include(s => s.Calendar)
                .FirstOrDefaultAsync(s => s.Id == shareId);

            if (share == null)
                throw new Exception("Sdílení nenalezeno");

            if (share.Calendar == null || share.Calendar.OwnerId != ownerId)
                throw new Exception("Nemáš oprávnění zrušit sdílení");

            _db.CalendarShares.Remove(share);
            await _db.SaveChangesAsync();
        }
    }
}
