namespace Semestralka.Presentation.Models.DTOs
{
    public class NotificationCreateDto
    {
        public Guid EventId { get; set; }
        public DateTimeOffset SendTime { get; set; }
    }
}
