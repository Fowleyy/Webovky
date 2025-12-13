namespace Semestralka.DTOs
{
    public class NotificationCreateDto
    {
        public Guid EventId { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
