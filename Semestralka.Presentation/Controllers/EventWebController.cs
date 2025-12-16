using Microsoft.AspNetCore.Mvc;
using Semestralka.DTOs.Event;
using Semestralka.Infrastructure.Services;

namespace Semestralka.Presentation.Controllers;

[Route("event")]
public class EventWebController : Controller
{
    private readonly EventService _eventService;

    public EventWebController(EventService eventService)
    {
        _eventService = eventService;
    }

    [HttpGet("create/{calendarId}")]
    public IActionResult Create(Guid calendarId)
    {
        ViewBag.CalendarId = calendarId;
        return View();
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        bool isAdmin =
            HttpContext.Session.GetString("isAdmin") == "1";

        await _eventService.CreateAsync(
            dto,
            Guid.Parse(uid),
            isAdmin
        );

        return Redirect("/");
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        bool isAdmin =
            HttpContext.Session.GetString("isAdmin") == "1";

        await _eventService.DeleteAsync(
            id,
            Guid.Parse(uid),
            isAdmin
        );

        return Redirect("/");
    }
}
