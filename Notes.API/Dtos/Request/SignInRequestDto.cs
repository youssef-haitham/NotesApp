using System.ComponentModel.DataAnnotations;

namespace NotesApp.API.Dtos.Request
{
    public class SignInRequestDto
    {
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required, MinLength(8), MaxLength(32)]
        public required string Password { get; set; }
    }
}
