using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using ToDo_Backend.DTOs;
using Xunit;

namespace ToDo_BackendTests.DTOs;

/// <summary>
/// Tests data-annotation validation on <see cref="UpdateToDoRequest"/>.
/// </summary>
public class UpdateToDoRequestValidationTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static IList<ValidationResult> Validate(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    // -----------------------------------------------------------------------
    // Title validation
    // -----------------------------------------------------------------------

    [Fact]
    public void UpdateToDoRequest_ValidRequest_PassesValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = "Updated Title", IsCompleted = false };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void UpdateToDoRequest_EmptyTitle_FailsValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = string.Empty };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateToDoRequest_NullTitle_FailsValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = null! };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateToDoRequest_TitleExceeds200Chars_FailsValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = new string('z', 201) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateToDoRequest_TitleExactly200Chars_PassesValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = new string('z', 200) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // Description validation
    // -----------------------------------------------------------------------

    [Fact]
    public void UpdateToDoRequest_DescriptionExceeds2000Chars_FailsValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = "Valid", Description = new string('d', 2001) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void UpdateToDoRequest_DescriptionExactly2000Chars_PassesValidation()
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = "Valid", Description = new string('d', 2000) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // IsCompleted flag
    // -----------------------------------------------------------------------

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void UpdateToDoRequest_IsCompletedEitherValue_PassesValidation(bool isCompleted)
    {
        // Arrange
        var request = new UpdateToDoRequest { Title = "Task", IsCompleted = isCompleted };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }
}
