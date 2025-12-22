using System.ComponentModel.DataAnnotations;

namespace NotesApp.API.Modules.Auth.Dtos.Request
{
    public class UpdatePasswordRequestDto
    {
        [Required]
        [MinLength(8), MaxLength(32)]
        public required string CurrentPassword { get; set; }

        [Required]
        [MinLength(8), MaxLength(32)]
        public required string NewPassword { get; set; }
    }
}

