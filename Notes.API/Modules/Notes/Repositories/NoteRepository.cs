using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.DBContext;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Modules.Notes.Interfaces.Repositories;

namespace NotesApp.API.Modules.Notes.Repositories
{
    public class NoteRepository(NoteDBContext noteDBContext) : INoteRepository
    {
        private readonly NoteDBContext _noteDBContext = noteDBContext;

        public async Task<Note> AddNote(Note note)
        {
            var addedNote = await _noteDBContext.Note.AddAsync(note);
            await _noteDBContext.SaveChangesAsync();
            return addedNote.Entity;
        }

        public async Task<Note?> UpdateNote(Note note)
        {
            var existingNote = await _noteDBContext.Note
                .FirstOrDefaultAsync(n => n.Id == note.Id && !n.IsDeleted);

            if (existingNote == null)
            {
                return null;
            }

            existingNote.Title = note.Title;
            existingNote.Content = note.Content;
            existingNote.BackgroundColor = note.BackgroundColor;
            existingNote.UpdatedAt = DateTime.UtcNow;

            await _noteDBContext.SaveChangesAsync();
            return existingNote;
        }

        public async Task<IEnumerable<Note>> GetUserNotes(Guid userId)
        {
            return await _noteDBContext.Note
                .Where(n => n.UserId == userId && !n.IsDeleted)
                .OrderByDescending(n => n.UpdatedAt)
                .ToListAsync();
        }

        public async Task<Note?> GetNoteById(Guid noteId)
        {
            return await _noteDBContext.Note
                .FirstOrDefaultAsync(n => n.Id == noteId && !n.IsDeleted);
        }

        public async Task<bool> DeleteNote(Guid noteId)
        {
            var note = await _noteDBContext.Note
                .FirstOrDefaultAsync(n => n.Id == noteId && !n.IsDeleted);

            if (note == null)
            {
                return false;
            }

            note.IsDeleted = true;
            note.UpdatedAt = DateTime.UtcNow;
            await _noteDBContext.SaveChangesAsync();

            return true;
        }
    }
}
