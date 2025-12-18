using System.ComponentModel.DataAnnotations;

namespace NotesApp.API.Modules.Auth.Dtos.Request
{
    public class SignUpRequestDto
    {
        [Required]
        [MinLength(3), MaxLength(32)]
        public required string Name { get; set; }
        [Required]
        [EmailAddress]
        public required string Email { get; set; }
        [Required, MinLength(8), MaxLength(32)]
        public required string Password { get; set; }
    }
}