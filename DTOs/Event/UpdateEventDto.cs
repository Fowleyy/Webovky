namespace Semestralka.DTOs.Event
{
    public class UpdateEventDto
    {
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Location { get; set; }
        public bool IsAllDay { get; set; }
    }
}
