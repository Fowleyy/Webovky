using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Semestralka.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? PasswordHash { get; set; }
        public string? TimeZone { get; set; }
        public string? AvatarPath { get; set; }
        public bool IsAdmin { get; set; } = false;


        public ICollection<Calendar> Calendars { get; set; } = new List<Calendar>();
        public ICollection<CalendarShare> SharedCalendars { get; set; } = new List<CalendarShare>();
    }
}
