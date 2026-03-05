using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using ToDoBackend.DTOs;

namespace ToDoBackend.Tests.DTOs;

/// <summary>
/// Data-annotation validation tests for <see cref="UpdateToDoRequest"/>.
/// </summary>
public class UpdateToDoRequestValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void UpdateToDoRequest_WithValidTitle_PassesValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = "Updated title" };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void UpdateToDoRequest_WithEmptyTitle_FailsValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = string.Empty };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateToDoRequest_WithNullTitle_FailsValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = null! };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(UpdateToDoRequest.Title)));
    }

    [Fact]
    public void UpdateToDoRequest_IsCompleted_DefaultsToFalse()
    {
        // Arrange & Act
        var request = new UpdateToDoRequest { Title = "Title" };

        // Assert
        request.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void UpdateToDoRequest_CanSetIsCompletedToTrue()
    {
        // Arrange & Act
        var request = new UpdateToDoRequest { Title = "Title", IsCompleted = true };

        // Assert
        request.IsCompleted.Should().BeTrue();
    }
}
