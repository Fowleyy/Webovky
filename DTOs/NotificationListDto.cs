namespace Semestralka.DTOs
{
    public class NotificationListDto
    {
        public Guid Id { get; set; }
        public Guid EventId { get; set; }
        public DateTimeOffset SendTime { get; set; }
        public bool IsSent { get; set; }
    }
}
