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
            // Pokud je uživatel už přihlášen, rovnou domů
            if (HttpContext.Session.GetString("userid") != null)
                return Redirect("/");

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

            HttpContext.Session.SetString("userid", user.Id.ToString());
            HttpContext.Session.SetString("email", user.Email);
            HttpContext.Session.SetString("fullname", user.FullName ?? "Uživatel");
            HttpContext.Session.SetString("avatar", user.AvatarPath ?? "/avatars/default.png");
            HttpContext.Session.SetString("isAdmin", user.IsAdmin ? "1" : "0");

            return Redirect("/");
        }

        [HttpGet("/register")]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost("/register")]
        public async Task<IActionResult> RegisterPost(string fullname, string email, string password)
        {
            if (string.IsNullOrWhiteSpace(fullname) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(password))
            {
                ViewData["Error"] = "Vyplňte všechna pole.";
                return View("Register");
            }

            email = email.Trim().ToLower();

            if (password.Length < 6)
            {
                ViewData["Error"] = "Heslo musí mít alespoň 6 znaků.";
                return View("Register");
            }

            if (await _db.Users.AnyAsync(x => x.Email == email))
            {
                ViewData["Error"] = "Účet s tímto emailem již existuje.";
                return View("Register");
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                FullName = fullname,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                AvatarPath = "/avatars/default.png"
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            HttpContext.Session.SetString("userid", user.Id.ToString());
            HttpContext.Session.SetString("email", user.Email);
            HttpContext.Session.SetString("fullname", user.FullName);
            HttpContext.Session.SetString("avatar", user.AvatarPath);

            return Redirect("/");
        }


        [HttpGet("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Redirect("/login");
        }

    }
}
