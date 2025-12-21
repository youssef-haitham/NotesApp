using NotesApp.API.Modules.Notes.Enums;

namespace NotesApp.API.Modules.Notes.Dtos.Response
{
    public class NoteDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public Colors BackgroundColor { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}

