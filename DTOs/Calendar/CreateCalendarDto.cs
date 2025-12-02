namespace Semestralka.DTOs.Calendar
{
    public class CreateCalendarDto
    {
        public string Title { get; set; } = null!;
        public string? Color { get; set; }
        public string Visibility { get; set; } = "private"; // "private", "public", "shared"
    }
}
