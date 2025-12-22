using Moq;
using NotesApp.API.Common.Exceptions;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Modules.Auth.Services;

namespace NotesApp.API.Tests.Modules.Auth.Services;

[TestFixture]
public class UserServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private UserService _userService = null!;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _userService = new UserService(_userRepositoryMock.Object);
    }

    [Test]
    public async Task GetUserById_WhenUserExists_ShouldReturnUserDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Test User",
            Email = "test@example.com",
            PasswordHash = "hashed_password"
        };

        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync(user);

        // Act
        var result = await _userService.GetUserById(userId);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Id, Is.EqualTo(userId));
        Assert.That(result.Email, Is.EqualTo(user.Email));
        Assert.That(result.Name, Is.EqualTo(user.Name));
    }

    [Test]
    public async Task GetUserById_WhenUserDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();

        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(userId))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<NotFoundException>(async () => await _userService.GetUserById(userId));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task GetUsersAsync_WithValidPagination_ShouldReturnPagedResponse()
    {
        // Arrange
        var users = new List<User>
        {
            new User
            {
                Id = Guid.NewGuid(),
                Name = "User 1",
                Email = "user1@example.com",
                PasswordHash = "hash1"
            },
            new User
            {
                Id = Guid.NewGuid(),
                Name = "User 2",
                Email = "user2@example.com",
                PasswordHash = "hash2"
            }
        };

        _userRepositoryMock.Setup(x => x.GetUsersAsync(1, 10))
            .ReturnsAsync((users, 2));

        // Act
        var result = await _userService.GetUsersAsync(1, 10);

        // Assert
        Assert.That(result, Is.Not.Null);
        Assert.That(result.Data.Count(), Is.EqualTo(2));
        Assert.That(result.TotalCount, Is.EqualTo(2));
        Assert.That(result.PageNumber, Is.EqualTo(1));
        Assert.That(result.PageSize, Is.EqualTo(10));
    }

    [Test]
    public async Task GetUsersAsync_WithInvalidPageNumber_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var ex = Assert.ThrowsAsync<BadRequestException>(async () => await _userService.GetUsersAsync(0, 10));
        Assert.That(ex?.Message, Is.EqualTo("Page number must be greater than 0"));
    }

    [Test]
    public async Task GetUsersAsync_WithInvalidPageSize_ShouldThrowBadRequestException()
    {
        // Act & Assert
        var ex1 = Assert.ThrowsAsync<BadRequestException>(async () => await _userService.GetUsersAsync(1, 0));
        Assert.That(ex1?.Message, Is.EqualTo("Page size must be between 1 and 100"));

        var ex2 = Assert.ThrowsAsync<BadRequestException>(async () => await _userService.GetUsersAsync(1, 101));
        Assert.That(ex2?.Message, Is.EqualTo("Page size must be between 1 and 100"));
    }
}

