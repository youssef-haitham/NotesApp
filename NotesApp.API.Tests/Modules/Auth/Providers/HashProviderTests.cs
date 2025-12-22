using NotesApp.API.Modules.Auth.Utility;

namespace NotesApp.API.Tests.Modules.Auth.Providers;

[TestFixture]
public class HashProviderTests
{
    private HashProvider _hashProvider = null!;

    [SetUp]
    public void Setup()
    {
        _hashProvider = new HashProvider();
    }

    [Test]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hashed = _hashProvider.HashPassword(password);

        // Assert
        Assert.That(hashed, Is.Not.Null);
        Assert.That(hashed, Is.Not.Empty);
        Assert.That(hashed, Is.Not.EqualTo(password));
    }

    [Test]
    public void HashPassword_WithSamePassword_ShouldReturnDifferentHashes()
    {
        // Arrange
        var password = "TestPassword123";

        // Act
        var hash1 = _hashProvider.HashPassword(password);
        var hash2 = _hashProvider.HashPassword(password);

        // Assert
        Assert.That(hash1, Is.Not.EqualTo(hash2));
    }

    [Test]
    public void Verify_WithCorrectPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "TestPassword123";
        var hashed = _hashProvider.HashPassword(password);

        // Act
        var isValid = _hashProvider.Verify(password, hashed);

        // Assert
        Assert.That(isValid, Is.True);
    }

    [Test]
    public void Verify_WithIncorrectPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var wrongPassword = "WrongPassword";
        var hashed = _hashProvider.HashPassword(password);

        // Act
        var isValid = _hashProvider.Verify(wrongPassword, hashed);

        // Assert
        Assert.That(isValid, Is.False);
    }

    [Test]
    public void Verify_WithDifferentHashedPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "TestPassword123";
        var hashed1 = _hashProvider.HashPassword(password);
        var hashed2 = _hashProvider.HashPassword("DifferentPassword");

        // Act
        var isValid = _hashProvider.Verify(password, hashed2);

        // Assert
        Assert.That(isValid, Is.False);
    }
}

