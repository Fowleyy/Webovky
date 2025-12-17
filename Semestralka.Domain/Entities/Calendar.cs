using System.ComponentModel.DataAnnotations;

namespace Semestralka.Domain.Entities;

public class Calendar
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid OwnerId { get; set; }

    public User? Owner { get; set; }

    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Barva musí být ve formátu HEX")]
    public string? Color { get; set; }

    [Required]
    [RegularExpression("public|private", ErrorMessage = "Viditelnost musí být public nebo private")]
    public string? Visibility { get; set; }

    public ICollection<Event> Events { get; set; } = new List<Event>();
    public ICollection<CalendarShare> SharedWith { get; set; } = new List<CalendarShare>();
}
