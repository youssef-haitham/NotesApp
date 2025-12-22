using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.DBContext;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Modules.Auth.Repositories;
using NotesApp.API.Tests.Infrastructure.Helpers;

namespace NotesApp.API.Tests.Modules.Auth.Repositories;

[TestFixture]
public class UserRepositoryTests
{
    private NoteDBContext _context = null!;
    private UserRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextHelper.CreateInMemoryContext();
        _repository = new UserRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task AddUserAsync_ShouldAddUserToDatabase()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password"
        };

        // Act
        var result = await _repository.AddUserAsync(user);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.Not.EqualTo(Guid.Empty));
        Assert.That(await _context.User.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task GetUserByEmailAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password"
        };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByEmailAsync("test@example.com");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Email, Is.EqualTo("test@example.com"));
    }

    [Test]
    public async Task GetUserByEmailAsync_WhenUserDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetUserByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetUserByIdAsync_WhenUserExists_ShouldReturnUser()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password"
        };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserByIdAsync(user.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Id, Is.EqualTo(user.Id));
    }

    [Test]
    public async Task UserExistsByEmailAsync_WhenUserExists_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password"
        };
        await _context.User.AddAsync(user);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.UserExistsByEmailAsync("test@example.com");

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public async Task UserExistsByEmailAsync_WhenUserDoesNotExist_ShouldReturnFalse()
    {
        // Act
        var result = await _repository.UserExistsByEmailAsync("nonexistent@example.com");

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public async Task GetUsersAsync_ShouldReturnPagedUsers()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Email = "user1@example.com", Name = "User 1", PasswordHash = "hash1", CreatedAt = DateTime.UtcNow.AddDays(-2) },
            new User { Email = "user2@example.com", Name = "User 2", PasswordHash = "hash2", CreatedAt = DateTime.UtcNow.AddDays(-1) },
            new User { Email = "user3@example.com", Name = "User 3", PasswordHash = "hash3", CreatedAt = DateTime.UtcNow }
        };
        await _context.User.AddRangeAsync(users);
        await _context.SaveChangesAsync();

        // Act
        var (resultUsers, totalCount) = await _repository.GetUsersAsync(1, 2);

        // Assert
        Assert.That(totalCount, Is.EqualTo(3));
        Assert.That(resultUsers.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task AddUserRoleAsync_ShouldAddUserRole()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            Name = "Test User",
            PasswordHash = "hashed_password"
        };
        var role = new Role { Name = "User", Description = "User role" };
        await _context.User.AddAsync(user);
        await _context.Role.AddAsync(role);
        await _context.SaveChangesAsync();

        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = role.Id,
            User = user,
            Role = role
        };

        // Act
        var result = await _repository.AddUserRoleAsync(userRole);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(await _context.UserRole.CountAsync(), Is.EqualTo(1));
    }
}

