using System.ComponentModel.DataAnnotations;

namespace Semestralka.Domain.Entities;

public class Event : IValidatableObject
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid CalendarId { get; set; }

    public Calendar Calendar { get; set; } = null!;

    [Required(ErrorMessage = "Název události je povinný")]
    [StringLength(100, ErrorMessage = "Název události může mít maximálně 100 znaků")]
    public string Title { get; set; } = null!;

    [StringLength(500, ErrorMessage = "Popis může mít maximálně 500 znaků")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Čas začátku je povinný")]
    public DateTimeOffset StartTime { get; set; }

    [Required(ErrorMessage = "Čas konce je povinný")]
    public DateTimeOffset EndTime { get; set; }

    [StringLength(200, ErrorMessage = "Místo může mít maximálně 200 znaků")]
    public string? Location { get; set; }

    public bool IsAllDay { get; set; }

    public Guid? CategoryId { get; set; }
    public EventCategory? Category { get; set; }

    public ICollection<Participant> Participants { get; set; } = new List<Participant>();

    /// <summary>
    /// Vlastní serverová validace – konec nesmí být dříve než začátek
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime < StartTime)
        {
            yield return new ValidationResult(
                "Čas konce události nesmí být dříve než čas začátku",
                new[] { nameof(EndTime) }
            );
        }
    }
}
