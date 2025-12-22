using Microsoft.Extensions.Configuration;
using Moq;
using NotesApp.API.Infrastructure.Data;
using NotesApp.API.Infrastructure.Models;
using NotesApp.API.Interfaces.Repositories;
using NotesApp.API.Interfaces.Utility;
using NotesApp.API.Modules.Auth.Interfaces.Repositories;

namespace NotesApp.API.Tests.Infrastructure.Data;

[TestFixture]
public class DataSeederTests
{
    private Mock<IRoleRepository> _roleRepositoryMock = null!;
    private Mock<IUserRepository> _userRepositoryMock = null!;
    private Mock<IHashProvider> _hashProviderMock = null!;
    private Mock<IConfiguration> _configurationMock = null!;

    [SetUp]
    public void Setup()
    {
        _roleRepositoryMock = new Mock<IRoleRepository>();
        _userRepositoryMock = new Mock<IUserRepository>();
        _hashProviderMock = new Mock<IHashProvider>();
        _configurationMock = new Mock<IConfiguration>();
    }

    [Test]
    public async Task SeedRolesAsync_WhenNoRolesExist_ShouldAddRoles()
    {
        // Arrange
        _roleRepositoryMock.Setup(x => x.GetAllRolesAsync())
            .ReturnsAsync(new List<Role>());

        // Act
        await DataSeeder.SeedRolesAsync(_roleRepositoryMock.Object);

        // Assert
        _roleRepositoryMock.Verify(x => x.AddRolesAsync(It.Is<IEnumerable<Role>>(roles =>
            roles.Count() == 2 &&
            roles.Any(r => r.Name == "User" && r.Description == "Regular user role") &&
            roles.Any(r => r.Name == "Admin" && r.Description == "Administrator role")
        )), Times.Once);
    }

    [Test]
    public async Task SeedRolesAsync_WhenRolesExist_ShouldNotAddRoles()
    {
        // Arrange
        var existingRoles = new List<Role>
        {
            new Role { Id = 1, Name = "User" }
        };
        _roleRepositoryMock.Setup(x => x.GetAllRolesAsync())
            .ReturnsAsync(existingRoles);

        // Act
        await DataSeeder.SeedRolesAsync(_roleRepositoryMock.Object);

        // Assert
        _roleRepositoryMock.Verify(x => x.AddRolesAsync(It.IsAny<IEnumerable<Role>>()), Times.Never);
    }

    [Test]
    public async Task SeedAdminUserAsync_WhenAdminDoesNotExist_ShouldCreateAdminUser()
    {
        // Arrange
        var adminRole = new Role { Id = 1, Name = "Admin" };
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "admin@notesapp.com",
            Name = "Admin User",
            PasswordHash = "hashed_password"
        };

        _configurationMock.Setup(x => x["Admin:Email"]).Returns((string?)null);
        _configurationMock.Setup(x => x["Admin:Password"]).Returns((string?)null);
        _configurationMock.Setup(x => x["Admin:Name"]).Returns((string?)null);

        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync("admin@notesapp.com"))
            .ReturnsAsync(false);
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync("Admin"))
            .ReturnsAsync(adminRole);
        _hashProviderMock.Setup(x => x.HashPassword("Admin@123"))
            .Returns("hashed_password");
        _userRepositoryMock.Setup(x => x.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync(adminUser);

        // Act
        await DataSeeder.SeedAdminUserAsync(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _hashProviderMock.Object,
            _configurationMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.AddUserAsync(It.Is<User>(u =>
            u.Email == "admin@notesapp.com" &&
            u.Name == "Admin User"
        )), Times.Once);
        _userRepositoryMock.Verify(x => x.AddUserRoleAsync(It.Is<UserRole>(ur =>
            ur.UserId == adminUser.Id &&
            ur.RoleId == adminRole.Id
        )), Times.Once);
    }

    [Test]
    public async Task SeedAdminUserAsync_WhenAdminExists_ShouldNotCreateAdminUser()
    {
        // Arrange
        _configurationMock.Setup(x => x["Admin:Email"]).Returns("admin@notesapp.com");
        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync("admin@notesapp.com"))
            .ReturnsAsync(true);

        // Act
        await DataSeeder.SeedAdminUserAsync(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _hashProviderMock.Object,
            _configurationMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.AddUserAsync(It.IsAny<User>()), Times.Never);
    }

    [Test]
    public async Task SeedAdminUserAsync_WhenAdminRoleNotFound_ShouldThrowException()
    {
        // Arrange
        _configurationMock.Setup(x => x["Admin:Email"]).Returns("admin@notesapp.com");
        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync("admin@notesapp.com"))
            .ReturnsAsync(false);
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync("Admin"))
            .ReturnsAsync((Role?)null);

        // Act & Assert
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await DataSeeder.SeedAdminUserAsync(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _hashProviderMock.Object,
                _configurationMock.Object));

        Assert.That(ex?.Message, Is.EqualTo("Admin role not found. Please seed roles first."));
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldUseEnvironmentVariableOverConfiguration()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", "env@example.com");
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", "EnvPassword123");
        Environment.SetEnvironmentVariable("ADMIN_NAME", "Env Admin");

        var adminRole = new Role { Id = 1, Name = "Admin" };
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "env@example.com",
            Name = "Env Admin",
            PasswordHash = "hashed_password"
        };

        _configurationMock.Setup(x => x["Admin:Email"]).Returns("config@example.com");
        _configurationMock.Setup(x => x["Admin:Password"]).Returns("ConfigPassword123");
        _configurationMock.Setup(x => x["Admin:Name"]).Returns("Config Admin");

        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync("env@example.com"))
            .ReturnsAsync(false);
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync("Admin"))
            .ReturnsAsync(adminRole);
        _hashProviderMock.Setup(x => x.HashPassword("EnvPassword123"))
            .Returns("hashed_password");
        _userRepositoryMock.Setup(x => x.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync(adminUser);

        // Act
        await DataSeeder.SeedAdminUserAsync(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _hashProviderMock.Object,
            _configurationMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.AddUserAsync(It.Is<User>(u =>
            u.Email == "env@example.com" &&
            u.Name == "Env Admin"
        )), Times.Once);

        // Cleanup
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);
        Environment.SetEnvironmentVariable("ADMIN_NAME", null);
    }

    [Test]
    public async Task SeedAdminUserAsync_ShouldUseConfigurationWhenEnvironmentVariableNotSet()
    {
        // Arrange
        Environment.SetEnvironmentVariable("ADMIN_EMAIL", null);
        Environment.SetEnvironmentVariable("ADMIN_PASSWORD", null);
        Environment.SetEnvironmentVariable("ADMIN_NAME", null);

        var adminRole = new Role { Id = 1, Name = "Admin" };
        var adminUser = new User
        {
            Id = Guid.NewGuid(),
            Email = "config@example.com",
            Name = "Config Admin",
            PasswordHash = "hashed_password"
        };

        var configSection = new Mock<IConfigurationSection>();
        configSection.Setup(x => x["Email"]).Returns("config@example.com");
        configSection.Setup(x => x["Password"]).Returns("ConfigPassword123");
        configSection.Setup(x => x["Name"]).Returns("Config Admin");

        _configurationMock.Setup(x => x["Admin:Email"]).Returns("config@example.com");
        _configurationMock.Setup(x => x["Admin:Password"]).Returns("ConfigPassword123");
        _configurationMock.Setup(x => x["Admin:Name"]).Returns("Config Admin");

        _userRepositoryMock.Setup(x => x.UserExistsByEmailAsync("config@example.com"))
            .ReturnsAsync(false);
        _roleRepositoryMock.Setup(x => x.GetRoleByNameAsync("Admin"))
            .ReturnsAsync(adminRole);
        _hashProviderMock.Setup(x => x.HashPassword("ConfigPassword123"))
            .Returns("hashed_password");
        _userRepositoryMock.Setup(x => x.AddUserAsync(It.IsAny<User>()))
            .ReturnsAsync(adminUser);

        // Act
        await DataSeeder.SeedAdminUserAsync(
            _userRepositoryMock.Object,
            _roleRepositoryMock.Object,
            _hashProviderMock.Object,
            _configurationMock.Object);

        // Assert
        _userRepositoryMock.Verify(x => x.AddUserAsync(It.Is<User>(u =>
            u.Email == "config@example.com" &&
            u.Name == "Config Admin"
        )), Times.Once);
    }
}
