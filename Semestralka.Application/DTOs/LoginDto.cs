using System.ComponentModel.DataAnnotations;

namespace Semestralka.Presentation.Models.DTOs
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Email je povinný")]
        [EmailAddress(ErrorMessage = "Neplatný formát emailu")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Heslo je povinné")]
        public string Password { get; set; } = null!;
    }
}
