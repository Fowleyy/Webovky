using System.ComponentModel.DataAnnotations;

namespace Semestralka.Domain.Entities;

public class Notification
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public Guid UserId { get; set; }

    public Guid? EventId { get; set; }

    [Required(ErrorMessage = "Titulek notifikace je povinný")]
    [StringLength(100)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Text notifikace je povinný")]
    [StringLength(500)]
    public string Body { get; set; } = "";

    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Required(ErrorMessage = "Čas upozornění je povinný")]
    public DateTime NotifyAt { get; set; }
}
