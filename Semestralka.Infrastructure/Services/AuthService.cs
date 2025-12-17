using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Domain.Exceptions;
using Semestralka.Domain.Validations;
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

            UserValidator.ValidateRegister(
                email,
                dto.Password,
                dto.FullName!,
                dto.TimeZone
            );

            if (await _db.Users.AnyAsync(x => x.Email == email))
                throw new DomainValidationException("Email už existuje.");

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

            UserValidator.ValidateLogin(email, dto.Password);

            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                throw new DomainValidationException("Neplatné přihlašovací údaje.");

            return user;
        }
    }
}
