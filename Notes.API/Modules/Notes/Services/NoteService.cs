using NotesApp.API.Common.Exceptions;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Modules.Notes.Dtos.Request;
using NotesApp.API.Modules.Notes.Dtos.Response;
using NotesApp.API.Modules.Notes.Interfaces.Repositories;
using NotesApp.API.Modules.Notes.Interfaces.Services;

namespace NotesApp.API.Modules.Notes.Services
{
    public class NoteService(INoteRepository noteRepository) : INoteService
    {
        private readonly INoteRepository _noteRepository = noteRepository;

        public async Task<NoteDto> CreateNoteAsync(Guid userId, CreateNoteRequestDto request)
        {
            var note = new Note
            {
                Title = request.Title,
                Content = request.Content,
                BackgroundColor = request.BackgroundColor,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var createdNote = await _noteRepository.AddNote(note);
            return MapToDto(createdNote);
        }

        public async Task<NoteDto> UpdateNoteAsync(Guid userId, Guid noteId, UpdateNoteRequestDto request)
        {
            var existingNote = await _noteRepository.GetNoteById(noteId);

            if (existingNote == null)
            {
                throw new NotFoundException("Note", noteId);
            }

            if (existingNote.UserId != userId)
            {
                throw new ForbiddenException("You do not have permission to update this note");
            }

            existingNote.Title = request.Title;
            existingNote.Content = request.Content;
            existingNote.BackgroundColor = request.BackgroundColor;
            existingNote.UpdatedAt = DateTime.UtcNow;

            var updatedNote = await _noteRepository.UpdateNote(existingNote);

            if (updatedNote == null)
            {
                throw new NotFoundException("Note", noteId);
            }

            return MapToDto(updatedNote);
        }

        public async Task<IEnumerable<NoteDto>> GetUserNotesAsync(Guid userId)
        {
            var notes = await _noteRepository.GetUserNotes(userId);
            return notes.Select(MapToDto);
        }

        public async Task<NoteDto> GetNoteByIdAsync(Guid userId, Guid noteId)
        {
            var note = await _noteRepository.GetNoteById(noteId);

            if (note == null)
            {
                throw new NotFoundException("Note", noteId);
            }

            if (note.UserId != userId)
            {
                throw new ForbiddenException("You do not have permission to access this note");
            }

            return MapToDto(note);
        }

        public async Task DeleteNoteAsync(Guid userId, Guid noteId)
        {
            var note = await _noteRepository.GetNoteById(noteId);

            if (note == null)
            {
                throw new NotFoundException("Note", noteId);
            }

            if (note.UserId != userId)
            {
                throw new ForbiddenException("You do not have permission to delete this note");
            }

            var deleted = await _noteRepository.DeleteNote(noteId);

            if (!deleted)
            {
                throw new NotFoundException("Note", noteId);
            }
        }

        private static NoteDto MapToDto(Note note)
        {
            return new NoteDto
            {
                Id = note.Id,
                Title = note.Title,
                Content = note.Content,
                BackgroundColor = note.BackgroundColor,
                CreatedAt = note.CreatedAt,
                UpdatedAt = note.UpdatedAt
            };
        }
    }
}