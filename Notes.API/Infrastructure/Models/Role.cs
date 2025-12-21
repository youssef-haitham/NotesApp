using System.ComponentModel.DataAnnotations;

namespace NotesApp.API.Infrastructure.Models
{
    public class Role
    {
        [Required]
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(50)]
        public required string Name { get; set; }

        [MaxLength(200)]
        public string? Description { get; set; }

        [Required]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public List<UserRole> UserRoles { get; set; } = [];
    }
}

