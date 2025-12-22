using Moq;
using NotesApp.API.Common.Exceptions;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Modules.Notes.Dtos.Request;
using NotesApp.API.Modules.Notes.Enums;
using NotesApp.API.Modules.Notes.Interfaces.Repositories;
using NotesApp.API.Modules.Notes.Services;

namespace NotesApp.API.Tests.Modules.Notes.Services;

[TestFixture]
public class NoteServiceTests
{
    private Mock<INoteRepository> _noteRepositoryMock = null!;
    private NoteService _noteService = null!;

    [SetUp]
    public void Setup()
    {
        _noteRepositoryMock = new Mock<INoteRepository>();
        _noteService = new NoteService(_noteRepositoryMock.Object);
    }

    [Test]
    public async Task CreateNoteAsync_ShouldCreateAndReturnNote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var request = new CreateNoteRequestDto
        {
            Title = "Test Note",
            Content = "Test Content",
            BackgroundColor = Colors.YELLOW
        };

        var createdNote = new Note
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            BackgroundColor = request.BackgroundColor,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _noteRepositoryMock.Setup(x => x.AddNote(It.IsAny<Note>()))
            .ReturnsAsync(createdNote);

        // Act
        var result = await _noteService.CreateNoteAsync(userId, request);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(request.Title));
        Assert.That(result.Content, Is.EqualTo(request.Content));
        Assert.That(result.BackgroundColor, Is.EqualTo(request.BackgroundColor));
        _noteRepositoryMock.Verify(x => x.AddNote(It.Is<Note>(n => n.UserId == userId)), Times.Once);
    }

    [Test]
    public async Task GetUserNotesAsync_ShouldReturnUserNotes()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var notes = new List<Note>
        {
            new Note
            {
                Id = Guid.NewGuid(),
                Title = "Note 1",
                Content = "Content 1",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new Note
            {
                Id = Guid.NewGuid(),
                Title = "Note 2",
                Content = "Content 2",
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        };

        _noteRepositoryMock.Setup(x => x.GetUserNotes(userId))
            .ReturnsAsync(notes);

        // Act
        var result = await _noteService.GetUserNotesAsync(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task GetNoteByIdAsync_WhenNoteExistsAndBelongsToUser_ShouldReturnNote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var note = new Note
        {
            Id = noteId,
            Title = "Test Note",
            Content = "Test Content",
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync(note);

        // Act
        var result = await _noteService.GetNoteByIdAsync(userId, noteId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(noteId));
        Assert.That(result.Title, Is.EqualTo(note.Title));
    }

    [Test]
    public async Task GetNoteByIdAsync_WhenNoteDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync((Note?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _noteService.GetNoteByIdAsync(userId, noteId));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetNoteByIdAsync_WhenNoteBelongsToDifferentUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var note = new Note
        {
            Id = noteId,
            Title = "Test Note",
            UserId = otherUserId
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync(note);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ForbiddenException>(async () =>
            await _noteService.GetNoteByIdAsync(userId, noteId));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateNoteAsync_WhenNoteExistsAndBelongsToUser_ShouldUpdateAndReturnNote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var existingNote = new Note
        {
            Id = noteId,
            Title = "Old Title",
            Content = "Old Content",
            BackgroundColor = Colors.YELLOW,
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var updateRequest = new UpdateNoteRequestDto
        {
            Title = "New Title",
            Content = "New Content",
            BackgroundColor = Colors.BLUE
        };

        var updatedNote = new Note
        {
            Id = noteId,
            Title = updateRequest.Title,
            Content = updateRequest.Content,
            BackgroundColor = updateRequest.BackgroundColor,
            UserId = userId,
            CreatedAt = existingNote.CreatedAt,
            UpdatedAt = DateTime.UtcNow
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync(existingNote);
        _noteRepositoryMock.Setup(x => x.UpdateNote(It.IsAny<Note>()))
            .ReturnsAsync(updatedNote);

        // Act
        var result = await _noteService.UpdateNoteAsync(userId, noteId, updateRequest);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Title, Is.EqualTo(updateRequest.Title));
        Assert.That(result.Content, Is.EqualTo(updateRequest.Content));
        Assert.That(result.BackgroundColor, Is.EqualTo(updateRequest.BackgroundColor));
        _noteRepositoryMock.Verify(x => x.UpdateNote(It.IsAny<Note>()), Times.Once);
    }

    [Test]
    public async Task UpdateNoteAsync_WhenNoteDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var updateRequest = new UpdateNoteRequestDto
        {
            Title = "New Title",
            Content = "New Content"
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync((Note?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _noteService.UpdateNoteAsync(userId, noteId, updateRequest));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateNoteAsync_WhenNoteBelongsToDifferentUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var existingNote = new Note
        {
            Id = noteId,
            Title = "Test Note",
            UserId = otherUserId
        };

        var updateRequest = new UpdateNoteRequestDto
        {
            Title = "New Title",
            Content = "New Content"
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync(existingNote);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ForbiddenException>(async () =>
            await _noteService.UpdateNoteAsync(userId, noteId, updateRequest));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteNoteAsync_WhenNoteExistsAndBelongsToUser_ShouldDeleteNote()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var note = new Note
        {
            Id = noteId,
            Title = "Test Note",
            UserId = userId
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync(note);
        _noteRepositoryMock.Setup(x => x.DeleteNote(noteId))
            .ReturnsAsync(true);

        // Act
        await _noteService.DeleteNoteAsync(userId, noteId);

        // Assert
        _noteRepositoryMock.Verify(x => x.DeleteNote(noteId), Times.Once);
    }

    [Test]
    public async Task DeleteNoteAsync_WhenNoteDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync((Note?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _noteService.DeleteNoteAsync(userId, noteId));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteNoteAsync_WhenNoteBelongsToDifferentUser_ShouldThrowForbiddenException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var note = new Note
        {
            Id = noteId,
            Title = "Test Note",
            UserId = otherUserId
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync(note);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ForbiddenException>(async () =>
            await _noteService.DeleteNoteAsync(userId, noteId));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task DeleteNoteAsync_WhenDeleteFails_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        var note = new Note
        {
            Id = noteId,
            Title = "Test Note",
            UserId = userId
        };

        _noteRepositoryMock.Setup(x => x.GetNoteById(noteId))
            .ReturnsAsync(note);
        _noteRepositoryMock.Setup(x => x.DeleteNote(noteId))
            .ReturnsAsync(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _noteService.DeleteNoteAsync(userId, noteId));
        Assert.That(ex, Is.Not.Null);
    }
}

