using NotesApp.API.Common.Exceptions;

namespace NotesApp.API.Tests.Common.Exceptions;

[TestFixture]
public class NotFoundExceptionTests
{
    [Test]
    public void Constructor_WithMessage_ShouldSetProperties()
    {
        // Arrange
        var message = "Resource not found";

        // Act
        var exception = new NotFoundException(message);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.StatusCode, Is.EqualTo(404));
        Assert.That(exception.ErrorCode, Is.EqualTo("NOT_FOUND"));
    }

    [Test]
    public void Constructor_WithResourceNameAndKey_ShouldFormatMessage()
    {
        // Arrange
        var resourceName = "User";
        var key = Guid.NewGuid();

        // Act
        var exception = new NotFoundException(resourceName, key);

        // Assert
        Assert.That(exception.Message, Is.EqualTo($"User with id '{key}' was not found."));
        Assert.That(exception.StatusCode, Is.EqualTo(404));
        Assert.That(exception.ErrorCode, Is.EqualTo("NOT_FOUND"));
    }
}

