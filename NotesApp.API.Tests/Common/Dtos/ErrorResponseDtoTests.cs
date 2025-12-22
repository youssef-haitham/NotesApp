using NotesApp.API.Common.Dtos;

namespace NotesApp.API.Tests.Common.Dtos;

[TestFixture]
public class ErrorResponseDtoTests
{
    [Test]
    public void Constructor_ShouldInitializeWithDefaultValues()
    {
        // Act
        var dto = new ErrorResponseDto();

        // Assert
        Assert.That(dto.ErrorCode, Is.EqualTo(string.Empty));
        Assert.That(dto.Message, Is.EqualTo(string.Empty));
        Assert.That(dto.Errors, Is.Null);
        Assert.That(dto.TraceId, Is.Null);
        Assert.That(dto.Timestamp, Is.LessThanOrEqualTo(DateTime.UtcNow));
    }

    [Test]
    public void Properties_ShouldBeSettable()
    {
        // Arrange
        var dto = new ErrorResponseDto
        {
            ErrorCode = "TEST_ERROR",
            Message = "Test message",
            TraceId = "trace-123",
            Timestamp = DateTime.UtcNow
        };

        // Act & Assert
        Assert.That(dto.ErrorCode, Is.EqualTo("TEST_ERROR"));
        Assert.That(dto.Message, Is.EqualTo("Test message"));
        Assert.That(dto.TraceId, Is.EqualTo("trace-123"));
    }

    [Test]
    public void Errors_ShouldBeSettable()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Field1", new[] { "Error 1", "Error 2" } }
        };

        var dto = new ErrorResponseDto
        {
            Errors = errors
        };

        // Act & Assert
        Assert.That(dto.Errors, Is.EqualTo(errors));
    }
}