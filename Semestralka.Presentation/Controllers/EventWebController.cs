using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Application.DTOs.Event;
using Semestralka.Infrastructure.Services;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Presentation.Controllers;

[Route("events")]
public class EventWebController : Controller
{
    private readonly EventService _eventService;
    private readonly CalendarDbContext _db;

    public EventWebController(
        EventService eventService,
        CalendarDbContext db)
    {
        _eventService = eventService;
        _db = db;
    }

    // =========================
    // LIST
    // GET /events
    // =========================
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

        var ordered = allEvents
            .OrderBy(e => e.Start)
            .ToList();

        return View(ordered);
    }

    // =========================
    // CREATE - FORM
    // GET /events/create/{calendarId}
    // =========================
    [HttpGet("create/{calendarId}")]
    public IActionResult Create(Guid calendarId)
    {
        var model = new CreateEventDto
        {
            CalendarId = calendarId,
            Start = DateTimeOffset.Now,
            End = DateTimeOffset.Now.AddHours(1)
        };

        ViewBag.Categories = _db.EventCategories.ToList();
        return View(model);
    }

    // =========================
    // CREATE - SUBMIT
    // POST /events/create/{calendarId}
    // =========================
    [HttpPost("create/{calendarId}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Guid calendarId, CreateEventDto dto)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        dto.CalendarId = calendarId;
        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        if (!ModelState.IsValid)
        {
            ViewBag.Categories = _db.EventCategories.ToList();
            return View(dto);
        }

        await _eventService.CreateAsync(
        dto,
        Guid.Parse(uid),
        isAdmin
    );

    // 🔥 vždy si načti kalendář
    var calendar = await _db.Calendars
        .AsNoTracking()
        .FirstAsync(c => c.Id == calendarId);

    // 🔥 VŽDY OwnerId
    return Redirect($"/admin/calendar/{calendar.OwnerId}");
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

        var dto = new UpdateEventDto
        {
            Id = ev.Id,
            Title = ev.Title,
            Description = ev.Description,
            Start = ev.StartTime,
            End = ev.EndTime,
            Location = ev.Location,
            IsAllDay = ev.IsAllDay,
            CategoryId = ev.CategoryId
        };

        ViewBag.Categories = _db.EventCategories.ToList();
        return View(dto);
    }

    [HttpPost("edit")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UpdateEventDto dto)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        Guid userId = Guid.Parse(uid);
        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        var ev = await _db.Events
            .Include(e => e.Calendar)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == dto.Id);

        if (ev == null)
            return NotFound();

        await _eventService.UpdateAsync(
            dto,
            userId,
            isAdmin
        );

        if (isAdmin)
        {
            return Redirect($"/admin/calendar/{ev.Calendar.OwnerId}");
        }

        return Redirect($"/calendar/{ev.CalendarId}");
    }

    [HttpGet("delete/{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        Guid userId = Guid.Parse(uid);
        bool isAdmin = HttpContext.Session.GetString("isAdmin") == "1";

        var ev = await _db.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);

        if (ev == null)
            return Redirect("/events");

        await _eventService.DeleteAsync(id, userId, isAdmin);

        if (isAdmin)
            return Redirect($"/admin/calendar/{ev.CalendarId}");

        return Redirect($"/calendar/{ev.CalendarId}");
    }
}
