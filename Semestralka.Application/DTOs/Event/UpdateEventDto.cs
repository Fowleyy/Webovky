using System.ComponentModel.DataAnnotations;

namespace Semestralka.Application.DTOs.Event;

public class UpdateEventDto : IValidatableObject
{
    [Required]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Název události je povinný")]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    [Required]
    public DateTimeOffset Start { get; set; }

    [Required]
    public DateTimeOffset End { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    public bool IsAllDay { get; set; }

    public Guid? CategoryId { get; set; }

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
