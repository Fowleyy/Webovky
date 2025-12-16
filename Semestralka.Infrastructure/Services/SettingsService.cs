using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Infrastructure.Services
{
    public class SettingsService
    {
        private readonly CalendarDbContext _db;

        public SettingsService(CalendarDbContext db)
        {
            _db = db;
        }

        public async Task<User> GetUserAsync(Guid userId)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Uživatel nenalezen");

            return user;
        }

        public async Task UpdateTimeZoneAsync(Guid userId, string timeZone)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Uživatel nenalezen");

            user.TimeZone = timeZone;
            await _db.SaveChangesAsync();
        }

        public async Task UpdateProfileAsync(Guid userId, string fullName)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null)
                throw new Exception("Uživatel nenalezen");

            user.FullName = fullName;
            await _db.SaveChangesAsync();
        }
    }
}
