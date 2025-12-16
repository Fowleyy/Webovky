using Microsoft.AspNetCore.Mvc;
using Semestralka.Infrastructure.Services;

namespace Semestralka.Presentation.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly EventService _eventService;

    public EventsController(EventService eventService)
    {
        _eventService = eventService;
    }

    // === FULLCALENDAR LOAD ===
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] Guid calendarId)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin =
            HttpContext.Session.GetString("isAdmin") == "1";

        var events = await _eventService.GetEventsAsync(
            calendarId,
            Guid.Parse(uid),
            isAdmin
        );

        return Ok(events);
    }
}
