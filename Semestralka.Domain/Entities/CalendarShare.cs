using System.ComponentModel.DataAnnotations;

namespace Semestralka.Domain.Entities;

public class CalendarShare
{
    [Key]
    public Guid Id { get; set; }

    public Guid CalendarId { get; set; }
    public Calendar? Calendar { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public string Permission { get; set; } = "read";
}
