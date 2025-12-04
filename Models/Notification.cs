using System.ComponentModel.DataAnnotations;

namespace Semestralka.Models
{
    public class Notification
    {
        [Key]
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public Guid? EventId { get; set; }

        public string Title { get; set; } = "";
        public string Body { get; set; } = "";

        public bool IsRead { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime NotifyAt { get; set; }
    }
}
