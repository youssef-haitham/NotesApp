using Moq;
using NotesApp.API.Common.Exceptions;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Modules.Auth.Dtos.Request;
using NotesApp.API.Modules.Auth.Interfaces.Repositories;
using NotesApp.API.Modules.Auth.Services;

namespace NotesApp.API.Tests.Modules.Auth.Services;

[TestFixture]
public class AuthServiceTests
{
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IRoleRepository> _roleRepositoryMock = null!;
    private Mock<ITokenProvider> _tokenProviderMock = null!;
    private Mock<IHashProvider> _hashProviderMock = null!;
    private AuthService _authService = null!;

    [SetUp]
    public void Setup()
    {
        _userRepositoryMock = new Mock<IUserRepository>();
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _tokenProviderMock = new Mock<ITokenProvider>();
        _hashProviderMock = new Mock<IHashProvider>();

        _authService = new AuthService(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _tokenProviderMock.Object,
            _hashProviderMock.Object);
    }

    [Test]
    public async Task SignUp_WhenUserDoesNotExist_ShouldCreateUserAndReturnToken()
    {
        // Arrange
        var signUpRequest = new SignUpRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123"
        };

        var userRole = new Role { Id = 1, Name = "User" };
        var createdUser = new User
        {
            Id = Guid.NewGuid(),
            Email = signUpRequest.Email,
            Name = signUpRequest.Name,
            PasswordHash = "hashed_password"
        };

        var userWithRoles = new User
        {
            Id = createdUser.Id,
            Email = createdUser.Email,
            Name = createdUser.Name,
            PasswordHash = createdUser.PasswordHash,
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = userRole }
            }
        };

        _hashProviderMock.Setup(x => x.HashPassword(signUpRequest.Password))
            .Returns("hashed_password");
        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync(signUpRequest.Email))
            .ReturnsAsync(false);
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync("User"))
            .ReturnsAsync(userRole);
        _userRepositoryMock.Setup(x => x.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync(createdUser);
        _userRepositoryMock.Setup(x => x.GetUserByIdAsync(createdUser.Id))
            .ReturnsAsync(userWithRoles);
        _tokenProviderMock.Setup(x => x.CreateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test_token");

        // Act
        var result = await _authService.SignUp(signUpRequest);

        // Assert
        Assert.That(result.authResponse, Is.Not.Null);
        Assert.That(result.authResponse.Email, Is.EqualTo(signUpRequest.Email));
        Assert.That(result.authResponse.Name, Is.EqualTo(signUpRequest.Name));
        Assert.That(result.token, Is.EqualTo("test_token"));
        _userRepositoryMock.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Once);
        _userRepositoryMock.Verify(x => x.AddUserRoleAsync(It.IsAny<UserRole>()), Times.Once);
    }

    [Test]
    public async Task SignUp_WhenUserExists_ShouldThrowConflictException()
    {
        // Arrange
        var signUpRequest = new SignUpRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123"
        };

        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync(signUpRequest.Email))
            .ReturnsAsync(true);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ConflictException>(async () => await _authService.SignUp(signUpRequest));
        Assert.That(ex, Is.Not.Null);
        _userRepositoryMock.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task SignUp_WhenUserRoleNotFound_ShouldThrowException()
    {
        // Arrange
        var signUpRequest = new SignUpRequestDto
        {
            Name = "Test User",
            Email = "test@example.com",
            Password = "Password123"
        };

        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync(signUpRequest.Email))
            .ReturnsAsync(false);
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync("User"))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () => await _authService.SignUp(signUpRequest));
        Assert.That(ex?.Message, Is.EqualTo("User role not found in database. Please seed roles first."));
    }

    [Test]
    public async Task SignIn_WhenCredentialsAreValid_ShouldReturnToken()
    {
        // Arrange
        var signInRequest = new SignInRequestDto
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        var userRole = new Role { Id = 1, Name = "User" };
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = signInRequest.Email,
            Name = "Test User",
            PasswordHash = "hashed_password",
            UserRoles = new List<UserRole>
            {
                new UserRole { Role = userRole }
            }
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(signInRequest.Email))
            .ReturnsAsync(existingUser);
        _hashProviderMock.Setup(x => x.Verify(signInRequest.Password, existingUser.PasswordHash))
            .Returns(true);
        _tokenProviderMock.Setup(x => x.CreateToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("test_token");

        // Act
        var result = await _authService.SignIn(signInRequest);

        // Assert
        Assert.That(result.authResponse, Is.Not.Null);
        Assert.That(result.authResponse.Email, Is.EqualTo(signInRequest.Email));
        Assert.That(result.token, Is.EqualTo("test_token"));
    }

    [Test]
    public async Task SignIn_WhenUserDoesNotExist_ShouldThrowBadRequestException()
    {
        // Arrange
        var signInRequest = new SignInRequestDto
        {
            Email = "test@example.com",
            Password = "Password123"
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(signInRequest.Email))
            .ReturnsAsync((User?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadRequestException>(async () => await _authService.SignIn(signInRequest));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task SignIn_WhenPasswordIsInvalid_ShouldThrowBadRequestException()
    {
        // Arrange
        var signInRequest = new SignInRequestDto
        {
            Email = "test@example.com",
            Password = "WrongPassword"
        };

        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            Email = signInRequest.Email,
            Name = "Test User",
            PasswordHash = "hashed_password"
        };

        _userRepositoryMock.Setup(x => x.GetUserByEmailAsync(signInRequest.Email))
            .ReturnsAsync(existingUser);
        _hashProviderMock.Setup(x => x.Verify(signInRequest.Password, existingUser.PasswordHash))
            .Returns(false);

        // Act & Assert
        var ex = Assert.ThrowsAsync<BadRequestException>(async () => await _authService.SignIn(signInRequest));
        Assert.That(ex, Is.Not.Null);
    }
}

