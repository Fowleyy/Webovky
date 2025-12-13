using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Semestralka.Data;
using Semestralka.DTOs;
using Semestralka.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Semestralka.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly CalendarDbContext _db;
        private const string JwtKey = "THIS_IS_THE_FINAL_TEST_KEY_1234567890_ABCDEF_0987654321";

        public AuthController(CalendarDbContext db)
        {
            _db = db;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email))
                return BadRequest();

            var email = dto.Email.Trim().ToLower();

            if (await _db.Users.AnyAsync(x => x.Email == email))
                return Conflict();

            if (dto.Password == null || dto.Password.Length < 6)
                return BadRequest();

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

            return Ok();
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest();

            var email = dto.Email.Trim().ToLower();

            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized();

            var token = GenerateToken(user);

            HttpContext.Session.SetString("userid", user.Id.ToString());
            HttpContext.Session.SetString("email", user.Email ?? "");
            HttpContext.Session.SetString("fullname", user.FullName ?? "Uživatel");

            return Ok(new { token });
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userid", user.Id.ToString()),
                new Claim("email", user.Email!)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
