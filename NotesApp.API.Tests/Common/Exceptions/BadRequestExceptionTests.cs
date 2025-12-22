using NotesApp.API.Common.Exceptions;

namespace NotesApp.API.Tests.Common.Exceptions;

[TestFixture]
public class BadRequestExceptionTests
{
    [Test]
    public void Constructor_WithMessage_ShouldSetProperties()
    {
        // Arrange
        var message = "Invalid request";

        // Act
        var exception = new BadRequestException(message);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.StatusCode, Is.EqualTo(400));
        Assert.That(exception.ErrorCode, Is.EqualTo("BAD_REQUEST"));
    }

    [Test]
    public void Constructor_WithMessageAndInnerException_ShouldSetProperties()
    {
        // Arrange
        var message = "Invalid request";
        var innerException = new Exception("Inner error");

        // Act
        var exception = new BadRequestException(message, innerException);

        // Assert
        Assert.That(exception.Message, Is.EqualTo(message));
        Assert.That(exception.InnerException, Is.EqualTo(innerException));
        Assert.That(exception.StatusCode, Is.EqualTo(400));
        Assert.That(exception.ErrorCode, Is.EqualTo("BAD_REQUEST"));
    }
}