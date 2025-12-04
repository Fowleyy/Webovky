namespace Semestralka.DTOs
{
    public class CalendarCreateDto
    {
        public string Title { get; set; } = null!;
        public string? Color { get; set; }
        public string Visibility { get; set; } = "private";
    }
}
