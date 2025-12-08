using System;
using System.Collections.Generic;

namespace Semestralka.Models
{
    public class Event
    {
        public Guid Id { get; set; }
        public Guid CalendarId { get; set; }
        public Calendar Calendar { get; set; } = null!;

        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public DateTimeOffset StartTime { get; set; }
        public DateTimeOffset EndTime { get; set; }
        public string? Location { get; set; }
        public bool IsAllDay { get; set; }

        public Guid? CategoryId { get; set; }
        public EventCategory? Category { get; set; }

        public ICollection<Participant> Participants { get; set; } = new List<Participant>();
    }
}
