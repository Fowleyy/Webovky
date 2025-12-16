namespace Semestralka.Domain.Entities;

public class EventCategory
{
    public Guid Id { get; set; }

    public string Name { get; set; } = "";

    public string ColorHex { get; set; } = "#6b46c1";

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
