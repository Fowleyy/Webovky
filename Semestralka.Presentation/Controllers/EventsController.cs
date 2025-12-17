using Microsoft.AspNetCore.Mvc;
using Semestralka.Domain.Exceptions;
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
    public async Task<IActionResult> Create(CreateEventDto dto)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        try
        {
            await _eventService.CreateAsync(
                dto,
                Guid.Parse(uid),
                isAdmin
            );

            return Ok();
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    public async Task<IActionResult> Update(UpdateEventDto dto)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        try
        {
            await _eventService.UpdateAsync(
                dto,
                Guid.Parse(uid),
                isAdmin
            );

            return Ok();
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Unauthorized();

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        try
        {
            await _eventService.DeleteAsync(
                id,
                Guid.Parse(uid),
                isAdmin
            );

            return Ok();
        }
        catch (DomainValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
