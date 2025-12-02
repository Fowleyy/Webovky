using System;

namespace Semestralka.Models
{
    public class Notification
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public Guid? EventId { get; set; }
        public Event? Event { get; set; }
        public string Type { get; set; } = "Email";
        public DateTimeOffset SendTime { get; set; }
        public bool IsSent { get; set; }
    }
}
