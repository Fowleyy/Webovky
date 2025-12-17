using System.ComponentModel.DataAnnotations;

namespace Semestralka.Presentation.Models.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Email je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Heslo je povinné")]
        [MinLength(6, ErrorMessage = "Heslo musí mít alespoň 6 znaků")]
        public string Password { get; set; } = null!;

        [Required(ErrorMessage = "Jméno je povinné")]
        [StringLength(100)]
        public string? FullName { get; set; }

        [Required]
        public string TimeZone { get; set; } = "Europe/Prague";
    }
}
