using NotesApp.API.Common.Exceptions;

namespace NotesApp.API.Tests.Common.Exceptions;

[TestFixture]
public class ConflictExceptionTests
{
    [Test]
    public void Constructor_WithMessage_ShouldSetProperties()
    {
        // Arrange
        var message = "Resource already exists";

        // Act
        var exception = new ConflictException(message);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.StatusCode, Is.EqualTo(409));
        Assert.That(exception.ErrorCode, Is.EqualTo("CONFLICT"));
    }

    [Test]
    public void Constructor_WithResourceNameFieldAndValue_ShouldFormatMessage()
    {
        // Arrange
        var resourceName = "User";
        var field = "email";
        var value = "test@example.com";

        // Act
        var exception = new ConflictException(resourceName, field, value);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"User with email 'test@example.com' already exists."));
        Assert.That(exception.StatusCode, Is.EqualTo(409));
        Assert.That(exception.ErrorCode, Is.EqualTo("CONFLICT"));
    }
}

