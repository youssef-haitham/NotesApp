using NotesApp.API.Common.Exceptions;

namespace NotesApp.API.Tests.Common.Exceptions;

[TestFixture]
public class ValidationExceptionTests
{
    [Test]
    public void Constructor_Default_ShouldInitializeEmptyErrors()
    {
        // Act
        var exception = new ValidationException();

        // Assert
        Assert.That(exception.Message, Is.EqualTo("One or more validation errors occurred."));
        Assert.That(exception.StatusCode, Is.EqualTo(400));
        Assert.That(exception.ErrorCode, Is.EqualTo("VALIDATION_ERROR"));
        Assert.That(exception.Errors, Is.Not.Null);
        Assert.That(exception.Errors, Is.Empty);
    }

    [Test]
    public void Constructor_WithErrorsDictionary_ShouldSetErrors()
    {
        // Arrange
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required", "Email format is invalid" } },
            { "Password", new[] { "Password must be at least 8 characters" } }
        };

        // Act
        var exception = new ValidationException(errors);

        // Assert
        Assert.That(exception.Errors, Is.EqualTo(errors));
        Assert.That(exception.Errors.Count, Is.EqualTo(2));
    }

    [Test]
    public void Constructor_WithFieldAndMessage_ShouldCreateSingleError()
    {
        // Arrange
        var field = "Email";
        var message = "Email is required";

        // Act
        var exception = new ValidationException(field, message);

        // Assert
        Assert.That(exception.Errors, Is.Not.Null);
        Assert.That(exception.Errors.Count, Is.EqualTo(1));
        Assert.That(exception.Errors.ContainsKey(field), Is.True);
        Assert.That(exception.Errors[field], Contains.Item(message));
    }
}

