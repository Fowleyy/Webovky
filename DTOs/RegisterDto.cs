namespace Semestralka.DTOs
{
    public class RegisterDto
    {
        public string Email { get; set; } = null!;
        public string Password { get; set; } = null!;
        public string? FullName { get; set; }
        public string TimeZone { get; set; } = "Europe/Prague";
    }
}
