namespace Semestralka.DTOs.Event
{
    public class EventReadDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = "";
        public DateTimeOffset Start { get; set; }
        public DateTimeOffset End { get; set; }
        public bool AllDay { get; set; }
    }
}
