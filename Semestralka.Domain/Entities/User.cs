using System.ComponentModel.DataAnnotations;

namespace Semestralka.Domain.Entities;

public class User
{
    [Key]
    public Guid Id { get; set; }

    [StringLength(100)]
    public string? FullName { get; set; }

    [Required(ErrorMessage = "Email je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    public string? Email { get; set; }

    [Required]
    public string? PasswordHash { get; set; }

    public string? TimeZone { get; set; }
    public string? AvatarPath { get; set; }

    public bool IsAdmin { get; set; } = false;

    public ICollection<Calendar> Calendars { get; set; } = new List<Calendar>();
    public ICollection<CalendarShare> SharedCalendars { get; set; } = new List<CalendarShare>();
}
