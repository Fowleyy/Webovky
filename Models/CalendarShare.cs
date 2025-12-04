using System.ComponentModel.DataAnnotations;

namespace Semestralka.Models
{
    public class CalendarShare
    {
        [Key]
        public Guid Id { get; set; }

        public Guid CalendarId { get; set; }
        public Calendar Calendar { get; set; }

        public Guid UserId { get; set; }      // komu je sdíleno
        public User User { get; set; }

        public string Permission { get; set; } = "read"; // read / edit
    }
}
