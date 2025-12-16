using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Application.DTOs.Calendar;
using Semestralka.Domain.Entities;
using Semestralka.Infrastructure.Data.Persistence;

namespace Semestralka.Presentation.Controllers;

public class CalendarController : Controller
{
    private readonly CalendarDbContext _db;

    public CalendarController(CalendarDbContext db)
    {
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateCalendarDto dto)
    {
        var calendar = new Calendar
        {
            Id = Guid.NewGuid(),
            OwnerId = dto.OwnerId,
            Color = dto.Color,
            Visibility = dto.Visibility
        };

        _db.Calendars.Add(calendar);
        await _db.SaveChangesAsync();

        return RedirectToAction("Index", "Home");
    }

    [HttpPost]
    public async Task<IActionResult> Update(Guid id, UpdateCalendarDto dto)
    {
        var calendar = await _db.Calendars.FirstOrDefaultAsync(c => c.Id == id);
        if (calendar == null)
            return NotFound();

        calendar.Color = dto.Color;
        calendar.Visibility = dto.Visibility;

        await _db.SaveChangesAsync();
        return RedirectToAction("Index", "Home");
    }
}
