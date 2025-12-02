using System;
using System.Collections.Generic;

namespace Semestralka.Models
{
    public class Calendar
    {
        public Guid Id { get; set; }
        public Guid OwnerId { get; set; }
        public User Owner { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Color { get; set; }
        public string Visibility { get; set; } = "private";

        public ICollection<Event> Events { get; set; } = new List<Event>();
    }
}
