using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Semestralka.Data;
using Semestralka.Models;

namespace Semestralka.Controllers;

public class HomeController : Controller
{
    private readonly CalendarDbContext _db;
    private readonly ILogger<HomeController> _logger;

    public HomeController(CalendarDbContext db, ILogger<HomeController> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<IActionResult> Index()
    {
        // pokud není přihlášen → login
        var uid = HttpContext.Session.GetString("userid");
        if (uid == null)
            return Redirect("/login");

        var userId = Guid.Parse(uid);

        // najdi hlavní kalendář uživatele
        var calendar = await _db.Calendars
            .Include(c => c.Events)
            .FirstOrDefaultAsync(c => c.OwnerId == userId);

        // vytvoř hlavní kalendář pokud neexistuje
        if (calendar == null)
        {
            calendar = new Calendar
            {
                Id = Guid.NewGuid(),
                OwnerId = userId,
                Title = "Můj kalendář",
                Color = "#4287f5",
                Visibility = "private"
            };

            _db.Calendars.Add(calendar);
            await _db.SaveChangesAsync();
        }

        return View("FullCalendarHome", calendar);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { 
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier 
        });
    }
}
