using System.ComponentModel.DataAnnotations;

namespace Semestralka.Application.DTOs.Event;

public class CreateEventDto
{
    public Guid Id { get; set; }

    public Guid CalendarId { get; set; }

    [Required(ErrorMessage = "Název události je povinný")]
    [StringLength(100, ErrorMessage = "Název může mít max. 100 znaků")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset Start { get; set; }

    [Required]
    public DateTimeOffset End { get; set; }

    public string? Location { get; set; }

    public bool IsAllDay { get; set; }

    public Guid? CategoryId { get; set; }
}
