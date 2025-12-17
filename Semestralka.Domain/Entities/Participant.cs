using System.ComponentModel.DataAnnotations;

namespace Semestralka.Domain.Entities;

public class Participant
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid EventId { get; set; }

    public Event Event { get; set; } = null!;

    [Required(ErrorMessage = "Email účastníka je povinný")]
    [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
    public string Email { get; set; } = null!;

    [Required]
    [RegularExpression("Pending|Accepted|Declined", ErrorMessage = "Neplatný stav účastníka")]
    public string Status { get; set; } = "Pending";
}
