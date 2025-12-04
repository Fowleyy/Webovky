using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    [Route("settings")]
    public class SettingsController : Controller
    {
        private readonly CalendarDbContext _db;
        private readonly IWebHostEnvironment _env;

        public SettingsController(CalendarDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        private Guid? UserId =>
            HttpContext.Session.GetString("userid") is string id
                ? Guid.Parse(id)
                : null;

        private IActionResult? RequireLogin()
        {
            if (UserId == null)
                return Redirect("/login");

            return null;
        }

        // ===========================
        // GET /settings
        // ===========================
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId);
            return View(user);
        }

        // ===========================
        // POST /settings/update
        // ===========================
        [HttpPost("update")]
        public async Task<IActionResult> Update(User model, string newPassword, IFormFile? avatarUpload)
        {
            var auth = RequireLogin();
            if (auth != null) return auth;

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == UserId);

            if (user == null) return Unauthorized();

            // === Update profilovky ===
            if (avatarUpload != null && avatarUpload.Length > 0)
            {
                var folder = Path.Combine(_env.WebRootPath, "avatars");
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);

                var fileName = $"{user.Id}{Path.GetExtension(avatarUpload.FileName)}";
                var filePath = Path.Combine(folder, fileName);

                using (var stream = System.IO.File.Create(filePath))
                    await avatarUpload.CopyToAsync(stream);

                user.AvatarPath = "/avatars/" + fileName;
            }


            // === Update jména a emailu ===
            user.FullName = model.FullName;
            user.Email = model.Email;

            // === Změna hesla ===
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            await _db.SaveChangesAsync();

            // session refresh
            HttpContext.Session.SetString("fullname", user.FullName);
            HttpContext.Session.SetString("email", user.Email);

            TempData["Success"] = "Změny byly uloženy.";
            return Redirect("/settings");
        }
    }
}
