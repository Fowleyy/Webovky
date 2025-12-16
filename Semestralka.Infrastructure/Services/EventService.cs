using Microsoft.EntityFrameworkCore;
using Semestralka.Domain.Entities;
using Semestralka.Application.DTOs.Event;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Infrastructure.Services;

public class EventService
{
    private readonly CalendarDbContext _db;

    public EventService(CalendarDbContext db)
    {
        _db = db;
    }

    // === NAČTENÍ UDÁLOSTÍ (READ) ===
    public async Task<List<CreateEventDto>> GetEventsAsync(
        Guid calendarId,
        Guid userId,
        bool isAdmin
    )
    {
        var calendar = await _db.Calendars
            .Include(c => c.SharedWith)
            .FirstOrDefaultAsync(c => c.Id == calendarId);

        if (calendar == null)
            return new List<CreateEventDto>();

        if (!isAdmin &&
            calendar.OwnerId != userId &&
            !calendar.SharedWith.Any(s => s.UserId == userId))
        {
            return new List<CreateEventDto>();
        }

        return await _db.Events
            .Where(e => e.CalendarId == calendarId)
            .Select(e => new CreateEventDto
            {
                Id = e.Id,
                CalendarId = e.CalendarId,
                Title = e.Title,
                Description = e.Description,
                Start = e.StartTime,
                End = e.EndTime,
                Location = e.Location,
                IsAllDay = e.IsAllDay,
                CategoryId = e.CategoryId
            })
            .ToListAsync();
    }

    // === VYTVOŘENÍ UDÁLOSTI ===
    public async Task CreateAsync(
        CreateEventDto dto,
        Guid userId,
        bool isAdmin
    )
    {
        // === ZÁKLADNÍ VALIDACE ===
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Název události je povinný.");

        if (dto.End <= dto.Start)
            throw new ArgumentException("Konec události musí být po začátku.");

        var calendar = await _db.Calendars
            .Include(c => c.SharedWith)
            .FirstOrDefaultAsync(c => c.Id == dto.CalendarId);

        if (calendar == null)
            throw new ArgumentException("Kalendář neexistuje.");

        if (!isAdmin &&
            calendar.OwnerId != userId &&
            !calendar.SharedWith.Any(s =>
                s.UserId == userId && s.Permission == "edit"))
        {
            throw new ArgumentException("Nemáš oprávnění vytvářet události v tomto kalendáři.");
        }


        var ev = new Event
        {
            Id = Guid.NewGuid(),
            CalendarId = dto.CalendarId,
            Title = dto.Title.Trim(),
            Description = dto.Description,
            StartTime = dto.Start,
            EndTime = dto.End,
            Location = dto.Location,
            IsAllDay = dto.IsAllDay,
            CategoryId = dto.CategoryId
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();
    }



    // === UPDATE UDÁLOSTI ===
    public async Task UpdateAsync(
        UpdateEventDto dto,
        Guid userId,
        bool isAdmin
    )
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new ArgumentException("Název události je povinný.");

        if (dto.End <= dto.Start)
            throw new ArgumentException("Konec události musí být po začátku.");

        var ev = await _db.Events
            .Include(e => e.Calendar)
            .ThenInclude(c => c.SharedWith)
            .FirstOrDefaultAsync(e => e.Id == dto.Id);

        if (ev == null)
            throw new ArgumentException("Událost neexistuje.");

        if (!isAdmin &&
            ev.Calendar.OwnerId != userId &&
            !ev.Calendar.SharedWith.Any(s =>
                s.UserId == userId && s.Permission == "edit"))
        {
            throw new ArgumentException("Nemáš oprávnění upravit tuto událost.");
        }

        ev.Title = dto.Title.Trim();
        ev.Description = dto.Description;
        ev.StartTime = dto.Start;
        ev.EndTime = dto.End;
        ev.Location = dto.Location;
        ev.IsAllDay = dto.IsAllDay;
        ev.CategoryId = dto.CategoryId;

        await _db.SaveChangesAsync();
    }



    // === SMAZÁNÍ UDÁLOSTI ===
    public async Task DeleteAsync(
        Guid eventId,
        Guid userId,
        bool isAdmin
    )
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
