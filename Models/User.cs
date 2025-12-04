using System;
using System.Collections.Generic;

namespace Semestralka.Models
{
    public class User
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string? FullName { get; set; }
        public string TimeZone { get; set; } = "Europe/Prague";
        public string? AvatarPath { get; set; } = "/avatars/default.png";

        public ICollection<Calendar> Calendars { get; set; } = new List<Calendar>();
    }
}
