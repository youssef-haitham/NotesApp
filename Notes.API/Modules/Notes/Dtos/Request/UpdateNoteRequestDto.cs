using NotesApp.API.Modules.Notes.Enums;
using System.ComponentModel.DataAnnotations;

namespace NotesApp.API.Modules.Notes.Dtos.Request
{
    public class UpdateNoteRequestDto
    {
        [Required]
        [MinLength(1), MaxLength(200)]
        public required string Title { get; set; }

        [MaxLength(5000)]
        public string Content { get; set; } = string.Empty;

        public Colors BackgroundColor { get; set; }
    }
}

