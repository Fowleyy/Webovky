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
                return BadRequest(new { message = "Email is required." });

            var email = dto.Email.Trim().ToLower();

            if (!email.Contains("@"))
                return BadRequest(new { message = "Invalid email format." });

            if (await _db.Users.AnyAsync(x => x.Email == email))
                return Conflict(new { message = "Email already exists." });

            if (dto.Password == null || dto.Password.Length < 6)
                return BadRequest(new { message = "Password must be at least 6 characters." });

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

            return Ok(new { message = "User created successfully." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Email) || string.IsNullOrWhiteSpace(dto.Password))
                return BadRequest(new { message = "Email and password are required." });

            var email = dto.Email.Trim().ToLower();

            var user = await _db.Users.SingleOrDefaultAsync(x => x.Email == email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials." });

            if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
                return Unauthorized(new { message = "Invalid credentials." });

            var token = GenerateToken(user);

            HttpContext.Session.SetString("userid", user.Id.ToString());
            HttpContext.Session.SetString("email", user.Email);
            HttpContext.Session.SetString("fullname", user.FullName ?? "Uživatel");

            return Ok(new
            {
                token,
                user = new
                {
                    user.Id,
                    user.Email,
                    user.FullName,
                    user.TimeZone
                }
            });
        }

        private string GenerateToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("userid", user.Id.ToString()),
                new Claim("email", user.Email)
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
