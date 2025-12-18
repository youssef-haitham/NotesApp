using NotesApp.API.Modules.Notes.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace NotesApp.API.Infrastructure.Models
{
    public class Note
    {
        [Required]
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public required string Title { get; set; }

        public string Content { get; set; } = string.Empty;

        [Required]
        public Colors BackgroundColor { get; set; } = Colors.YELLOW;

        public bool IsDeleted { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public Guid UserId { get; set; }

        [ForeignKey("UserId")]
        public required User User { get; set; }
    }
}
