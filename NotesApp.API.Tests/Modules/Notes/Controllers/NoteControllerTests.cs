using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NotesApp.API.Common.Exceptions;
using NotesApp.API.Modules.Notes.Controllers;
using NotesApp.API.Modules.Notes.Dtos.Request;
using NotesApp.API.Modules.Notes.Dtos.Response;
using NotesApp.API.Modules.Notes.Enums;
using NotesApp.API.Modules.Notes.Interfaces.Services;
using System.Security.Claims;

namespace NotesApp.API.Tests.Modules.Notes.Controllers;

[TestFixture]
public class NoteControllerTests
{
    private Mock<ILogger<NoteController>> _loggerMock = null!;
    private Mock<INoteService> _noteServiceMock = null!;
    private NoteController _noteController = null!;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<NoteController>>();
        _noteServiceMock = new Mock<INoteService>();

        _noteController = new NoteController(
            _loggerMock.Object,
            _noteServiceMock.Object)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private void SetUserClaims(Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim("id", userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        _noteController.ControllerContext.HttpContext.User = principal;
    }

    [Test]
    public async Task CreateNote_WhenUserIdExists_ShouldReturnCreated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserClaims(userId);

        var request = new CreateNoteRequestDto
        {
            Title = "Test Note",
            Content = "Test Content",
            BackgroundColor = Colors.YELLOW
        };

        var noteDto = new NoteDto
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            BackgroundColor = request.BackgroundColor
        };

        _noteServiceMock.Setup(x => x.CreateNoteAsync(userId, request))
            .ReturnsAsync(noteDto);

        // Act
        var result = await _noteController.CreateNote(request);

        // Assert
        Assert.That(result, Is.InstanceOf<CreatedAtActionResult>());
    }

    [Test]
    public async Task CreateNote_WhenUserIdNotFound_ShouldReturnUnauthorized()
    {
        // Arrange
        var request = new CreateNoteRequestDto
        {
            Title = "Test Note",
            Content = "Test Content"
        };

        // Act
        var result = await _noteController.CreateNote(request);

        // Assert
        Assert.That(result, Is.InstanceOf<UnauthorizedResult>());
    }

    [Test]
    public async Task GetUserNotes_WhenUserIdExists_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        SetUserClaims(userId);

        var notes = new List<NoteDto>
        {
            new NoteDto { Id = Guid.NewGuid(), Title = "Note 1" },
            new NoteDto { Id = Guid.NewGuid(), Title = "Note 2" }
        };

        _noteServiceMock.Setup(x => x.GetUserNotesAsync(userId))
            .ReturnsAsync(notes);

        // Act
        var result = await _noteController.GetUserNotes();

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetNoteById_WhenNoteExists_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        SetUserClaims(userId);

        var noteDto = new NoteDto
        {
            Id = noteId,
            Title = "Test Note"
        };

        _noteServiceMock.Setup(x => x.GetNoteByIdAsync(userId, noteId))
            .ReturnsAsync(noteDto);

        // Act
        var result = await _noteController.GetNoteById(noteId);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task GetNoteById_WhenNoteNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        SetUserClaims(userId);

        _noteServiceMock.Setup(x => x.GetNoteByIdAsync(userId, noteId))
            .ThrowsAsync(new NotFoundException("Note", noteId));

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () =>
            await _noteController.GetNoteById(noteId));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task UpdateNote_WhenNoteExists_ShouldReturnOk()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        SetUserClaims(userId);

        var request = new UpdateNoteRequestDto
        {
            Title = "Updated Title",
            Content = "Updated Content"
        };

        var noteDto = new NoteDto
        {
            Id = noteId,
            Title = request.Title,
            Content = request.Content
        };

        _noteServiceMock.Setup(x => x.UpdateNoteAsync(userId, noteId, request))
            .ReturnsAsync(noteDto);

        // Act
        var result = await _noteController.UpdateNote(noteId, request);

        // Assert
        Assert.That(result, Is.InstanceOf<OkObjectResult>());
    }

    [Test]
    public async Task DeleteNote_WhenNoteExists_ShouldReturnNoContent()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var noteId = Guid.NewGuid();
        SetUserClaims(userId);

        _noteServiceMock.Setup(x => x.DeleteNoteAsync(userId, noteId))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _noteController.DeleteNote(noteId);

        // Assert
        Assert.That(result, Is.InstanceOf<NoContentResult>());
    }
}

