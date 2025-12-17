using System.ComponentModel.DataAnnotations;

namespace Semestralka.Presentation.Models.DTOs
{
    public class NotificationCreateDto
    {
        [Required]
        public Guid EventId { get; set; }

        [Required(ErrorMessage = "Čas upozornění je povinný")]
        public DateTimeOffset SendTime { get; set; }
    }
}
