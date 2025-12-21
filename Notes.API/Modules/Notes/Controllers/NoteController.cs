using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NotesApp.API.Modules.Notes.Dtos.Request;
using NotesApp.API.Modules.Notes.Interfaces.Services;

namespace NotesApp.API.Modules.Notes.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "UserOrAdmin")]
    public class NoteController(ILogger<NoteController> logger, INoteService noteService) : ControllerBase
    {
        private readonly ILogger<NoteController> _logger = logger;
        private readonly INoteService _noteService = noteService;

        [HttpPost]
        public async Task<IActionResult> CreateNote([FromBody] CreateNoteRequestDto request)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("CreateNote: Creating note for user {UserId}", userId);
            var note = await _noteService.CreateNoteAsync(userId.Value, request);

            _logger.LogInformation("CreateNote: Note created successfully with id {NoteId}", note.Id);
            return CreatedAtAction(nameof(GetNoteById), new { id = note.Id }, note);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserNotes()
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("GetUserNotes: Retrieving notes for user {UserId}", userId);
            var notes = await _noteService.GetUserNotesAsync(userId.Value);

            return Ok(notes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNoteById(Guid id)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("GetNoteById: Retrieving note {NoteId} for user {UserId}", id, userId);
            var note = await _noteService.GetNoteByIdAsync(userId.Value, id);

            return Ok(note);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateNote(Guid id, [FromBody] UpdateNoteRequestDto request)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("UpdateNote: Updating note {NoteId} for user {UserId}", id, userId);
            var note = await _noteService.UpdateNoteAsync(userId.Value, id, request);

            _logger.LogInformation("UpdateNote: Note {NoteId} updated successfully", id);
            return Ok(note);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteNote(Guid id)
        {
            var userId = GetUserId();
            if (userId == null)
            {
                return Unauthorized();
            }

            _logger.LogInformation("DeleteNote: Deleting note {NoteId} for user {UserId}", id, userId);
            await _noteService.DeleteNoteAsync(userId.Value, id);

            _logger.LogInformation("DeleteNote: Note {NoteId} deleted successfully", id);
            return NoContent();
        }

        private Guid? GetUserId()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return null;
            }
            return userId;
        }
    }
}

