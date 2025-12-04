using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers
{
    public class AccountController : Controller
    {
        private readonly CalendarDbContext _db;

        public AccountController(CalendarDbContext db)
        {
            _db = db;
        }

        [HttpGet("/login")]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost("/login")]
        public async Task<IActionResult> LoginPost(string email, string password)
        {
            var user = await _db.Users.FirstOrDefaultAsync(x => x.Email == email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                ViewData["Error"] = "Špatný email nebo heslo.";
                return View("Login");
            }

            // === SESSION VALUES ===
            HttpContext.Session.SetString("userid", user.Id.ToString());
            HttpContext.Session.SetString("email", user.Email);
            HttpContext.Session.SetString("fullname", user.FullName ?? "Uživatel");

            // === AVATAR (DŮLEŽITÉ) ===
            HttpContext.Session.SetString("avatar", user.AvatarPath ?? "/avatars/default.png");

            return Redirect("/");
        }

        [HttpGet("/account/logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/login");
        }
    }
}
