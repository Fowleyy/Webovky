using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    public class SettingsController : Controller
    {
        private readonly CalendarDbContext _db;
        private readonly IWebHostEnvironment _env;

        public SettingsController(CalendarDbContext db, IWebHostEnvironment env)
        {
            _db = db;
            _env = env;
        }

        [HttpGet("/settings")]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("userid");
            if (userId == null) return Redirect("/login");

            var user = await _db.Users.FirstOrDefaultAsync(x => x.Id == Guid.Parse(userId));
            return View(user);
        }

        [HttpPost("/settings/update")]
        public async Task<IActionResult> Update(
            string FullName,
            string Email,
            string? oldPassword,
            string? newPassword,
            string? newPasswordConfirm,
            IFormFile? avatarUpload)
        {
            var userId = HttpContext.Session.GetString("userid");
            if (userId == null) return Redirect("/login");

            var user = await _db.Users.FirstAsync(x => x.Id == Guid.Parse(userId));

            if (avatarUpload != null && avatarUpload.Length > 0)
            {
                var uploadPath = Path.Combine(_env.WebRootPath, "avatars");

                if (!Directory.Exists(uploadPath))
                    Directory.CreateDirectory(uploadPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(avatarUpload.FileName)}";
                var fullPath = Path.Combine(uploadPath, fileName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await avatarUpload.CopyToAsync(stream);
                }

                user.AvatarPath = "/avatars/" + fileName;
                HttpContext.Session.SetString("avatar", user.AvatarPath);
            }

            user.FullName = FullName;
            user.Email = Email;

            HttpContext.Session.SetString("fullname", FullName);
            HttpContext.Session.SetString("email", Email);

            if (!string.IsNullOrEmpty(oldPassword) ||
                !string.IsNullOrEmpty(newPassword) ||
                !string.IsNullOrEmpty(newPasswordConfirm))
            {
                if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
                {
                    TempData["Error"] = "Staré heslo není správně.";
                    return Redirect("/settings");
                }

                if (newPassword != newPasswordConfirm)
                {
                    TempData["Error"] = "Nová hesla se neshodují.";
                    return Redirect("/settings");
                }

                if (newPassword!.Length < 6)
                {
                    TempData["Error"] = "Nové heslo musí mít alespoň 6 znaků.";
                    return Redirect("/settings");
                }

                user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = "Změny uloženy.";
            return Redirect("/settings");
        }
    }
}
