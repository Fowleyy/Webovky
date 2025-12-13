namespace Semestralka.Models
{
    public class EventCategory
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = "";

        // ✔ Barva kategorie (hex kód)
        public string ColorHex { get; set; } = "#6b46c1";

        // ✔ Navigation
        public List<Event> Events { get; set; } = new();
    }
}
