using Microsoft.AspNetCore.Mvc;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Services;

namespace Semestralka.Presentation.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventService _eventService;

        public EventsController(EventService eventService)
        {
            _eventService = eventService;
        }

        public async Task<IActionResult> Index()
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdStr);

            var events = await _eventService.GetUserEventsAsync(userId);
            return View(events);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Event ev)
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdStr);

            if (!ModelState.IsValid)
                return View(ev);

            try
            {
                await _eventService.CreateAsync(ev, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(ev);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Delete(Guid id)
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdStr);

            try
            {
                await _eventService.DeleteAsync(id, userId);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
