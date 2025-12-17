using System.ComponentModel.DataAnnotations;

namespace Semestralka.Application.DTOs.Event;

public class CreateEventDto : IValidatableObject
{
    public Guid Id { get; set; }

    [Required]
    public Guid CalendarId { get; set; }

    [Required(ErrorMessage = "Název události je povinný")]
    [StringLength(100, ErrorMessage = "Název může mít max. 100 znaků")]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required(ErrorMessage = "Začátek události je povinný")]
    public DateTimeOffset Start { get; set; }

    [Required(ErrorMessage = "Konec události je povinný")]
    public DateTimeOffset End { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public bool IsAllDay { get; set; }

    public Guid? CategoryId { get; set; }

    // 🔥 LOGICKÁ SERVEROVÁ VALIDACE
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (End < Start)
        {
            yield return new ValidationResult(
                "Konec události nesmí být dříve než začátek",
                new[] { nameof(End) }
            );
        }
    }
}
