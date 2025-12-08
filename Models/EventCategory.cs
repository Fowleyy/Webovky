namespace Semestralka.Models
{
    public class EventCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Color { get; set; } = "#6b46c1"; // default purple

        public ICollection<Event>? Events { get; set; }
    }
}
