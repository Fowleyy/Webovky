using Microsoft.AspNetCore.Mvc;
using Semestralka.Infrastructure.Services;
using Semestralka.Application.DTOs.Event;

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

    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] Guid calendarId,
        [FromQuery] DateTimeOffset? start,
        [FromQuery] DateTimeOffset? end)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        var events = await _eventService.GetEventsAsync(
            calendarId,
            Guid.Parse(uid),
            isAdmin
        );

        return Ok(events.Select(e => new
        {
            id = e.Id,
            title = e.Title,
            start = e.Start,
            end = e.End,
            allDay = e.IsAllDay
        }));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState); // 🔥 SERVEROVÁ VALIDACE

        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        if (dto.End < dto.Start)
            return BadRequest("Konec události nesmí být dříve než začátek.");

        await _eventService.CreateAsync(
            dto,
            Guid.Parse(uid),
            isAdmin
        );

        return Ok();
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateEventDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        if (dto.End < dto.Start)
            return BadRequest("Konec události nesmí být dříve než začátek.");

        await _eventService.UpdateAsync(
            dto,
            Guid.Parse(uid),
            isAdmin
        );

        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        await _eventService.DeleteAsync(
            id,
            Guid.Parse(uid),
            isAdmin
        );

        return Ok();
    }
}
