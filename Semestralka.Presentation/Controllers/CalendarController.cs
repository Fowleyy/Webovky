using Microsoft.AspNetCore.Mvc;
using Semestralka.Application.DTOs.Calendar;
using Semestralka.Infrastructure.Services;

namespace Semestralka.Presentation.Controllers
{
    public class CalendarController : Controller
    {
        private readonly CalendarService _calendarService;

        public CalendarController(CalendarService calendarService)
        {
            _calendarService = calendarService;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCalendarDto dto)
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdStr);

            try
            {
                await _calendarService.CreateIfNotExistsAsync(userId);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public async Task<IActionResult> Update(UpdateCalendarDto dto)
        {
            var userIdStr = HttpContext.Session.GetString("userid");
            if (string.IsNullOrEmpty(userIdStr))
                return RedirectToAction("Login", "Auth");

            var userId = Guid.Parse(userIdStr);

            try
            {
                if (!string.IsNullOrEmpty(dto.Color))
                {
                    await _calendarService.UpdateColorAsync(userId, dto.Color);
                }

                if (!string.IsNullOrEmpty(dto.Visibility))
                {
                    await _calendarService.UpdateVisibilityAsync(userId, dto.Visibility);
                }

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
