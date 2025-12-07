using NotesApp.API.Models;

namespace NotesApp.API.Interfaces.Repositories
{
    public interface INoteRepository
    {
        Task<Note> AddNote(Note note);
        Task<Note?> UpdateNote(Note note);
        Task<IEnumerable<Note>> GetUserNotes(Guid userId);
        Task<Note?> GetNoteById(Guid noteId);
        Task<bool> DeleteNote(Guid noteId);
    }
}
