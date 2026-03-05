using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using ToDoBackend.DTOs;

namespace ToDoBackend.Tests.DTOs;

/// <summary>
/// Data-annotation validation tests for <see cref="CreateToDoRequest"/>.
/// Uses the <see cref="Validator"/> class to simulate model binding validation.
/// </summary>
public class CreateToDoRequestValidationTests
{
    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    [Fact]
    public void CreateToDoRequest_WithValidTitle_PassesValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Valid title" };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateToDoRequest_WithEmptyTitle_FailsValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = string.Empty };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateToDoRequest_WithNullTitle_FailsValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = null! };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().NotBeEmpty();
        results.Should().Contain(r => r.MemberNames.Contains(nameof(CreateToDoRequest.Title)));
    }

    [Fact]
    public void CreateToDoRequest_WithOptionalDescriptionNull_PassesValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Title", Description = null };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().BeEmpty();
    }

    [Fact]
    public void CreateToDoRequest_WithOptionalDescriptionSet_PassesValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Title", Description = "Some notes" };

        // Act
        var results = Validate(request);

        // Assert
        results.Should().BeEmpty();
    }
}
