using NotesApp.API.Modules.Notes.Dtos.Request;
using NotesApp.API.Modules.Notes.Dtos.Response;

namespace NotesApp.API.Modules.Notes.Interfaces.Services
{
    public interface INoteService
    {
        Task<NoteDto> CreateNoteAsync(Guid userId, CreateNoteRequestDto request);
        Task<NoteDto> UpdateNoteAsync(Guid userId, Guid noteId, UpdateNoteRequestDto request);
        Task<IEnumerable<NoteDto>> GetUserNotesAsync(Guid userId);
        Task<NoteDto> GetNoteByIdAsync(Guid userId, Guid noteId);
        Task DeleteNoteAsync(Guid userId, Guid noteId);
    }
}

