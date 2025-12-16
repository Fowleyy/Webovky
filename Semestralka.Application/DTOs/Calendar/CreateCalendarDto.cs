namespace Semestralka.Application.DTOs.Calendar;

public class CreateCalendarDto
{
    public Guid OwnerId { get; set; }
    public string? Color { get; set; }
    public string? Visibility { get; set; }
}
