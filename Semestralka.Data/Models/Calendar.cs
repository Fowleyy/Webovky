using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Semestralka.Models
{
    public class Calendar
    {
        [Key]
        public Guid Id { get; set; }

        public Guid OwnerId { get; set; }

        public User? Owner { get; set; }

        public string? Color { get; set; }
        public string? Visibility { get; set; }

        public ICollection<Event> Events { get; set; } = new List<Event>();

        public ICollection<CalendarShare> SharedWith { get; set; } = new List<CalendarShare>();
    }
}
