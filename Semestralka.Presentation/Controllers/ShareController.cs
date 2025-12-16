using Microsoft.AspNetCore.Mvc;
using Semestralka.Infrastructure.Services;

namespace Semestralka.Presentation.Controllers
{
    public class ShareController : Controller
    {
        private readonly ShareService _shareService;

        public ShareController(ShareService shareService)
        {
            _shareService = shareService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(Guid userId, string permission)
        {
            var ownerIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(ownerIdStr))
                return RedirectToAction("Login", "Auth");

            var ownerId = Guid.Parse(ownerIdStr);

            try
            {
                await _shareService.ShareCalendarAsync(ownerId, userId, permission);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdStr);

            var shared = await _shareService.GetSharedCalendarsAsync(userId);
            return View(shared);
        }

        [HttpPost]
        public async Task<IActionResult> Remove(Guid id)
        {
            var ownerIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(ownerIdStr))
                return RedirectToAction("Login", "Auth");

            var ownerId = Guid.Parse(ownerIdStr);

            try
            {
                await _shareService.RemoveShareAsync(id, ownerId);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
