namespace Semestralka.DTOs.Calendar
{
    public class UpdateCalendarDto
    {
        public string Title { get; set; } = null!;
        public string? Color { get; set; }
        public string Visibility { get; set; } = "private";
    }
}
