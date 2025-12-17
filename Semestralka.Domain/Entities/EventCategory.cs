using System.ComponentModel.DataAnnotations;

namespace Semestralka.Domain.Entities;

public class EventCategory
{
    [Key]
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Název kategorie je povinný")]
    [StringLength(50)]
    public string Name { get; set; } = "";

    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Barva musí být ve formátu HEX (#RRGGBB)")]
    public string ColorHex { get; set; } = "#6b46c1";

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
