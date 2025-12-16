namespace Semestralka.Domain.Entities;

public class Participant
{
    public Guid Id { get; set; }

    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;

    public string Email { get; set; } = null!;
    public string Status { get; set; } = "Pending";
}
