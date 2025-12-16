using Microsoft.AspNetCore.Mvc;
using Semestralka.Infrastructure.Services;

namespace Semestralka.Presentation.Controllers
{
    [Route("settings")]
    public class SettingsController : Controller
    {
        private readonly SettingsService _settingsService;

        public SettingsController(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        // GET /settings
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return Redirect("/login");

            var userId = Guid.Parse(userIdStr);

            var user = await _settingsService.GetUserAsync(userId);
            if (user == null)
                return Unauthorized();

            return View(user);
        }

        // POST /settings/profile
        [HttpPost("profile")]
        public async Task<IActionResult> UpdateProfile(string fullName)
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return Redirect("/login");

            var userId = Guid.Parse(userIdStr);

            await _settingsService.UpdateProfileAsync(userId, fullName);
            return RedirectToAction("Index");
        }

        // POST /settings/timezone
        [HttpPost("timezone")]
        public async Task<IActionResult> UpdateTimeZone(string timeZone)
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return Redirect("/login");

            var userId = Guid.Parse(userIdStr);

            await _settingsService.UpdateTimeZoneAsync(userId, timeZone);
            return RedirectToAction("Index");
        }
    }
}
