using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Application.DTOs.Event;
using Semestralka.Domain.Exceptions;
using Semestralka.Infrastructure.Data.Persistence;
using Semestralka.Infrastructure.Services;

namespace Semestralka.Presentation.Controllers;

[Route("events")]
public class EventWebController : Controller
{
    private readonly EventService _eventService;
    private readonly CalendarDbContext _db;

    public EventWebController(EventService eventService, CalendarDbContext db)
    {
        _eventService = eventService;
        _db = db;
    }

    [HttpGet("")]
    public async Task<IActionResult> List()
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        Guid userId = Guid.Parse(uid);
        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        var calendars = await _db.Calendars
            .Include(c => c.SharedWith)
            .Where(c =>
                isAdmin ||
                c.OwnerId == userId ||
                c.SharedWith.Any(s => s.UserId == userId)
            )
            .ToListAsync();

        var allEvents = new List<CreateEventDto>();

        foreach (var cal in calendars)
        {
            var events = await _eventService.GetEventsAsync(
                cal.Id,
                userId,
                isAdmin
            );

            allEvents.AddRange(events);
        }

        return View(allEvents.OrderBy(e => e.Start).ToList());
    }

    [HttpGet("create/{calendarId}")]
    public IActionResult Create(Guid calendarId)
    {
        ViewBag.Categories = _db.EventCategories.ToList();

        return View(new CreateEventDto
        {
            CalendarId = calendarId,
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddHours(1)
        });
    }

    [HttpPost("create/{calendarId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid calendarId, CreateEventDto dto)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";
        dto.CalendarId = calendarId;

        try
        {
            await _eventService.CreateAsync(
                dto,
                Guid.Parse(uid),
                isAdmin
            );

            var calendar = await _db.Calendars
                .AsNoTracking()
                .FirstAsync(c => c.Id == calendarId);

            return Redirect($"/admin/calendar/{calendar.OwnerId}");
        }
        catch (DomainValidationException ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.Categories = _db.EventCategories.ToList();
            return View(dto);
        }
    }

    [HttpGet("edit/{id}")]
    public async Task<IActionResult> Edit(Guid id)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        Guid userId = Guid.Parse(uid);
        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        var ev = await _db.Events
            .Include(e => e.Calendar)
            .ThenInclude(c => c.SharedWith)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null)
            return NotFound();

        if (!isAdmin &&
            ev.Calendar.OwnerId != userId &&
            !ev.Calendar.SharedWith.Any(s =>
                s.UserId == userId && s.Permission == "edit"))
        {
            return Forbid();
        }

        ViewBag.Categories = _db.EventCategories.ToList();

        return View(new UpdateEventDto
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            Start = ev.StartTime,
            End = ev.EndTime,
            Location = ev.Location,
            IsAllDay = ev.IsAllDay,
            CategoryId = ev.CategoryId
        });
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateEventDto dto)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        try
        {
            await _eventService.UpdateAsync(
                dto,
                Guid.Parse(uid),
                isAdmin
            );

            var ev = await _db.Events
                .AsNoTracking()
                .Include(e => e.Calendar)
                .FirstAsync(e => e.Id == dto.Id);

            return isAdmin
                ? Redirect($"/admin/calendar/{ev.Calendar.OwnerId}")
                : Redirect($"/calendar/{ev.CalendarId}");
        }
        catch (DomainValidationException ex)
        {
            ViewBag.Error = ex.Message;
            ViewBag.Categories = _db.EventCategories.ToList();
            return View(dto);
        }
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        await _eventService.DeleteAsync(
            id,
            Guid.Parse(uid),
            isAdmin
        );

        return Redirect("/events");
    }
}
