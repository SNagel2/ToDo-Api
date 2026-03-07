using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using ToDo_Backend.DTOs;
using Xunit;

namespace ToDo_BackendTests.DTOs;

/// <summary>
/// Tests data-annotation validation on <see cref="CreateToDoRequest"/>.
/// </summary>
public class CreateToDoRequestValidationTests
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
    public void CreateToDoRequest_ValidTitleAndNoDescription_PassesValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Buy milk" };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void CreateToDoRequest_EmptyTitle_FailsValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = string.Empty };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateToDoRequest_NullTitle_FailsValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = null! };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
    }

    [Fact]
    public void CreateToDoRequest_TitleExceeds200Chars_FailsValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = new string('x', 201) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(CreateToDoRequest.Title)));
    }

    [Fact]
    public void CreateToDoRequest_TitleExactly200Chars_PassesValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = new string('x', 200) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // Description validation
    // -----------------------------------------------------------------------

    [Fact]
    public void CreateToDoRequest_DescriptionExceeds2000Chars_FailsValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Valid", Description = new string('d', 2001) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().NotBeEmpty();
        errors.Should().Contain(e => e.MemberNames.Contains(nameof(CreateToDoRequest.Description)));
    }

    [Fact]
    public void CreateToDoRequest_DescriptionExactly2000Chars_PassesValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Valid", Description = new string('d', 2000) };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }

    [Fact]
    public void CreateToDoRequest_NullDescription_PassesValidation()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Valid", Description = null };

        // Act
        var errors = Validate(request);

        // Assert
        errors.Should().BeEmpty();
    }
}
