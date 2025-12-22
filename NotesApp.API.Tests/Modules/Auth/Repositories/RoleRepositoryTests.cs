using Microsoft.EntityFrameworkCore;
using NotesApp.API.Infrastructure.DBContext;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Modules.Auth.Repositories;
using NotesApp.API.Tests.Infrastructure.Helpers;

namespace NotesApp.API.Tests.Modules.Auth.Repositories;

[TestFixture]
public class RoleRepositoryTests
{
    private NoteDBContext _context = null!;
    private RoleRepository _repository = null!;

    [SetUp]
    public void Setup()
    {
        _context = TestDbContextHelper.CreateInMemoryContext();
        _repository = new RoleRepository(_context);
    }

    [TearDown]
    public void TearDown()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    [Test]
    public async Task GetRoleByNameAsync_WhenRoleExists_ShouldReturnRole()
    {
        // Arrange
        var role = new Role
        {
            Name = "User",
            Description = "User role"
        };
        await _context.Role.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByNameAsync("User");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Name, Is.EqualTo("User"));
    }

    [Test]
    public async Task GetRoleByNameAsync_WhenRoleDoesNotExist_ShouldReturnNull()
    {
        // Act
        var result = await _repository.GetRoleByNameAsync("NonExistent");

        // Assert
        Assert.That(result, Is.Null);
    }

    [Test]
    public async Task GetRoleByNameAsync_ShouldBeCaseInsensitive()
    {
        // Arrange
        var role = new Role
        {
            Name = "User",
            Description = "User role"
        };
        await _context.Role.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByNameAsync("user");

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Name, Is.EqualTo("User"));
    }

    [Test]
    public async Task GetRoleByIdAsync_WhenRoleExists_ShouldReturnRole()
    {
        // Arrange
        var role = new Role
        {
            Name = "User",
            Description = "User role"
        };
        await _context.Role.AddAsync(role);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetRoleByIdAsync(role.Id);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result?.Id, Is.EqualTo(role.Id));
    }

    [Test]
    public async Task GetAllRolesAsync_ShouldReturnAllRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Name = "User", Description = "User role" },
            new Role { Name = "Admin", Description = "Admin role" }
        };
        await _context.Role.AddRangeAsync(roles);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllRolesAsync();

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
    }

    [Test]
    public async Task AddRoleAsync_ShouldAddRoleToDatabase()
    {
        // Arrange
        var role = new Role
        {
            Name = "User",
            Description = "User role"
        };

        // Act
        var result = await _repository.AddRoleAsync(role);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(await _context.Role.CountAsync(), Is.EqualTo(1));
    }

    [Test]
    public async Task AddRolesAsync_ShouldAddMultipleRoles()
    {
        // Arrange
        var roles = new List<Role>
        {
            new Role { Name = "User", Description = "User role" },
            new Role { Name = "Admin", Description = "Admin role" }
        };

        // Act
        var result = await _repository.AddRolesAsync(roles);

        // Assert
        Assert.That(result.Count(), Is.EqualTo(2));
        Assert.That(await _context.Role.CountAsync(), Is.EqualTo(2));
    }
}

