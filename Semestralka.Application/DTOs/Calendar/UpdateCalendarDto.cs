using System.ComponentModel.DataAnnotations;

namespace Semestralka.Application.DTOs.Calendar;

public class UpdateCalendarDto
{
    [RegularExpression("^#([0-9a-fA-F]{6})$", ErrorMessage = "Barva musí být ve formátu HEX")]
    public string? Color { get; set; }

    [RegularExpression("public|private", ErrorMessage = "Viditelnost musí být public nebo private")]
    public string? Visibility { get; set; }
}
