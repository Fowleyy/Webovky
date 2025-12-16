using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.DTOs.Event;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Infrastructure.Services;

public class EventService
{
    private readonly CalendarDbContext _db;

    public EventService(CalendarDbContext db)
    {
        _db = db;
    }

    // === NAČTENÍ UDÁLOSTÍ PRO FULLCALENDAR ===
    public async Task<List<EventReadDto>> GetEventsAsync(
        Guid calendarId,
        Guid userId,
        bool isAdmin
    )
    {
        var calendar = await _db.Calendars
            .Include(c => c.SharedWith)
            .FirstOrDefaultAsync(c =>
                c.Id == calendarId &&
                (
                    isAdmin ||
                    c.OwnerId == userId ||
                    c.SharedWith.Any(s => s.UserId == userId)
                )
            );

        if (calendar == null)
            return new List<EventReadDto>();

        return await _db.Events
            .Where(e => e.CalendarId == calendarId)
            .Select(e => new EventReadDto
            {
                Id = e.Id,
                Title = e.Title,
                Start = e.StartTime,
                End = e.EndTime,
                AllDay = e.IsAllDay
            })
            .ToListAsync();

    }

    // === VYTVOŘENÍ UDÁLOSTI ===
    public async Task CreateAsync(CreateEventDto dto, Guid userId, bool isAdmin)
    {
        var calendar = await _db.Calendars
            .Include(c => c.SharedWith)
            .FirstOrDefaultAsync(c =>
                c.Id == dto.CalendarId &&
                (
                    isAdmin ||
                    c.OwnerId == userId ||
                    c.SharedWith.Any(s =>
                        s.UserId == userId && s.Permission == "edit")
                )
            );

        if (calendar == null)
            throw new Exception("Kalendář neexistuje nebo k němu nemáš oprávnění");

        var ev = new Event
        {
            Id = Guid.NewGuid(),
            CalendarId = dto.CalendarId,
            Title = dto.Title,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Location = dto.Location,
            IsAllDay = dto.IsAllDay,
            CategoryId = dto.CategoryId
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
    }

    // === SMAZÁNÍ UDÁLOSTI ===
    public async Task DeleteAsync(Guid eventId, Guid userId, bool isAdmin)
    {
        var ev = await _db.Events
            .Include(e => e.Calendar)
            .ThenInclude(c => c.SharedWith)
            .FirstOrDefaultAsync(e => e.Id == eventId);

        if (ev == null)
            return;

        if (!isAdmin &&
            ev.Calendar.OwnerId != userId &&
            !ev.Calendar.SharedWith.Any(s =>
                s.UserId == userId && s.Permission == "edit"))
        {
            throw new Exception("Nemáš oprávnění mazat tuto událost");
        }

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
    }
}
