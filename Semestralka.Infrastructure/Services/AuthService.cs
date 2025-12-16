using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;
using Semestralka.Presentation.Models.DTOs;

namespace Semestralka.Infrastructure.Services
{
    public class AuthService
    {
        private readonly CalendarDbContext _db;

        public AuthService(CalendarDbContext db)
        {
            _db = db;
        }

        public async Task RegisterAsync(RegisterDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            if (await _db.Users.AnyAsync(x => x.Email == email))
                throw new Exception("Email už existuje");

            if (dto.Password == null || dto.Password.Length < 6)
                throw new Exception("Heslo musí mít alespoň 6 znaků");

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                FullName = dto.FullName,
                TimeZone = dto.TimeZone,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        public async Task<User> LoginAsync(LoginDto dto)
        {
            var email = dto.Email.Trim().ToLower();

            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new Exception("Neplatné přihlašovací údaje");

            return user;
        }
    }
}
