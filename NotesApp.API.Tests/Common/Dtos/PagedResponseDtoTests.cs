using NotesApp.API.Common.Dtos;

namespace NotesApp.API.Tests.Common.Dtos;

[TestFixture]
public class PagedResponseDtoTests
{
    [Test]
    public void TotalPages_WithExactDivision_ShouldReturnCorrectValue()
    {
        // Arrange
        var dto = new PagedResponseDto<int>
        {
            TotalCount = 20,
            PageSize = 10
        };

        // Act & Assert
        Assert.That(dto.TotalPages, Is.EqualTo(2));
    }

    [Test]
    public void TotalPages_WithRemainder_ShouldRoundUp()
    {
        // Arrange
        var dto = new PagedResponseDto<int>
        {
            TotalCount = 25,
            PageSize = 10
        };

        // Act & Assert
        Assert.That(dto.TotalPages, Is.EqualTo(3));
    }

    [Test]
    public void HasPrevious_OnFirstPage_ShouldReturnFalse()
    {
        // Arrange
        var dto = new PagedResponseDto<int>
        {
            PageNumber = 1,
            TotalCount = 20,
            PageSize = 10
        };

        // Act & Assert
        Assert.That(dto.HasPrevious, Is.False);
    }

    [Test]
    public void HasPrevious_OnSecondPage_ShouldReturnTrue()
    {
        // Arrange
        var dto = new PagedResponseDto<int>
        {
            PageNumber = 2,
            TotalCount = 20,
            PageSize = 10
        };

        // Act & Assert
        Assert.That(dto.HasPrevious, Is.True);
    }

    [Test]
    public void HasNext_OnLastPage_ShouldReturnFalse()
    {
        // Arrange
        var dto = new PagedResponseDto<int>
        {
            PageNumber = 2,
            TotalCount = 20,
            PageSize = 10
        };

        // Act & Assert
        Assert.That(dto.HasNext, Is.False);
    }

    [Test]
    public void HasNext_OnFirstPage_ShouldReturnTrue()
    {
        // Arrange
        var dto = new PagedResponseDto<int>
        {
            PageNumber = 1,
            TotalCount = 20,
            PageSize = 10
        };

        // Act & Assert
        Assert.That(dto.HasNext, Is.True);
    }

    [Test]
    public void HasNext_OnMiddlePage_ShouldReturnTrue()
    {
        // Arrange
        var dto = new PagedResponseDto<int>
        {
            PageNumber = 2,
            TotalCount = 30,
            PageSize = 10
        };

        // Act & Assert
        Assert.That(dto.HasNext, Is.True);
    }
}