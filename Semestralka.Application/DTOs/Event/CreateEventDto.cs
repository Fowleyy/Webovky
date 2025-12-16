namespace Semestralka.DTOs.Event
{
    public class CreateEventDto
    {
        public Guid CalendarId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public string? Location { get; set; }
        public bool IsAllDay { get; set; }
        public Guid? CategoryId { get; set; }
    }
}
