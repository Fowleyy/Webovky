namespace Semestralka.DTOs
{
    public class EventCreateDto
    {
        public Guid CalendarId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Location { get; set; }
        public bool IsAllDay { get; set; }
    }
}
