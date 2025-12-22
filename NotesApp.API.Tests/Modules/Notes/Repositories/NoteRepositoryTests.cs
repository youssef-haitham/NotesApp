using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.DBContext;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Modules.Notes.Enums;
using NotesApp.API.Modules.Notes.Repositories;
using NotesApp.API.Tests.Infrastructure.Helpers;

namespace NotesApp.API.Tests.Modules.Notes.Repositories;

[TestFixture]
public class NoteRepositoryTests
{
    private NoteDBContext _context = null!;
    private NoteRepository _repository = null!;
    private User _testUser = null!;

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextHelper.CreateInMemoryContext();
        _repository = new NoteRepository(_context);

        _testUser = new User
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password"
        };
        _context.User.Add(_testUser);
        _context.SaveChanges();
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddNote_ShouldAddNoteToDatabase()
    {
        // Arrange
        var note = new Note
        {
            Title = "Test Note",
            Content = "Test Content",
            BackgroundColor = Colors.YELLOW,
            UserId = _testUser.Id
        };

        // Act
        var result = await _repository.AddNote(note);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(await _context.Note.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetNoteById_WhenNoteExists_ShouldReturnNote()
    {
        // Arrange
        var note = new Note
        {
            Title = "Test Note",
            Content = "Test Content",
            UserId = _testUser.Id
        };
        await _context.Note.AddAsync(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNoteById(note.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Id, Is.EqualTo(note.Id));
    }

    [Test]
    public async Task GetNoteById_WhenNoteIsDeleted_ShouldReturnNull()
    {
        // Arrange
        var note = new Note
        {
            Title = "Test Note",
            Content = "Test Content",
            UserId = _testUser.Id,
            IsDeleted = true
        };
        await _context.Note.AddAsync(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetNoteById(note.Id);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserNotes_ShouldReturnOnlyUserNotes()
    {
        // Arrange
        var otherUser = new User
        {
            Email = "other@example.com",
            Name = "Other User",
            PasswordHash = "hash"
        };
        await _context.User.AddAsync(otherUser);
        await _context.SaveChangesAsync();

        var notes = new List<Note>
        {
            new Note { Title = "Note 1", UserId = _testUser.Id, UpdatedAt = DateTime.UtcNow.AddDays(-2) },
            new Note { Title = "Note 2", UserId = _testUser.Id, UpdatedAt = DateTime.UtcNow.AddDays(-1) },
            new Note { Title = "Note 3", UserId = otherUser.Id, UpdatedAt = DateTime.UtcNow }
        };
        await _context.Note.AddRangeAsync(notes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserNotes(_testUser.Id);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(result.All(n => n.UserId == _testUser.Id), Is.True);
    }

    [Test]
    public async Task GetUserNotes_ShouldOrderByUpdatedAtDescending()
    {
        // Arrange
        var notes = new List<Note>
        {
            new Note { Title = "Old Note", UserId = _testUser.Id, UpdatedAt = DateTime.UtcNow.AddDays(-2) },
            new Note { Title = "New Note", UserId = _testUser.Id, UpdatedAt = DateTime.UtcNow }
        };
        await _context.Note.AddRangeAsync(notes);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserNotes(_testUser.Id);

        // Assert
        Assert.That(result.First().Title, Is.EqualTo("New Note"));
        Assert.That(result.Last().Title, Is.EqualTo("Old Note"));
    }

    [Test]
    public async Task UpdateNote_WhenNoteExists_ShouldUpdateNote()
    {
        // Arrange
        var note = new Note
        {
            Title = "Original Title",
            Content = "Original Content",
            BackgroundColor = Colors.YELLOW,
            UserId = _testUser.Id
        };
        await _context.Note.AddAsync(note);
        await _context.SaveChangesAsync();

        note.Title = "Updated Title";
        note.Content = "Updated Content";
        note.BackgroundColor = Colors.BLUE;

        // Act
        var result = await _repository.UpdateNote(note);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Title, Is.EqualTo("Updated Title"));
        Assert.That(result?.Content, Is.EqualTo("Updated Content"));
        Assert.That(result?.BackgroundColor, Is.EqualTo(Colors.BLUE));
    }

    [Test]
    public async Task UpdateNote_WhenNoteDoesNotExist_ShouldReturnNull()
    {
        // Arrange
        var note = new Note
        {
            Id = Guid.NewGuid(),
            Title = "Non-existent Note",
            UserId = _testUser.Id
        };

        // Act
        var result = await _repository.UpdateNote(note);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task DeleteNote_WhenNoteExists_ShouldSoftDelete()
    {
        // Arrange
        var note = new Note
        {
            Title = "Test Note",
            Content = "Test Content",
            UserId = _testUser.Id
        };
        await _context.Note.AddAsync(note);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.DeleteNote(note.Id);

        // Assert
        Assert.That(result, Is.True);
        var deletedNote = await _context.Note.FindAsync(note.Id);
        Assert.That(deletedNote?.IsDeleted, Is.True);
    }

    [Test]
    public async Task DeleteNote_WhenNoteDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.DeleteNote(Guid.NewGuid());

        // Assert
        Assert.That(result, Is.False);
    }
}

