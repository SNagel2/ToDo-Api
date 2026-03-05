using FluentAssertions;
using ToDoBackend.Models;

namespace ToDoBackend.Tests.Models;

/// <summary>
/// Unit tests that verify default property values and basic invariants of <see cref="ToDoItem"/>.
/// </summary>
public class ToDoItemTests
{
    [Fact]
    public void ToDoItem_WhenCreated_HasNonEmptyId()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void ToDoItem_WhenCreated_IsNotCompleted()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void ToDoItem_WhenCreated_TitleIsEmptyString()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.Title.Should().Be(string.Empty);
    }

    [Fact]
    public void ToDoItem_WhenCreated_DescriptionIsNull()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.Description.Should().BeNull();
    }

    [Fact]
    public void ToDoItem_WhenCreated_CreatedAtUtcIsApproximatelyNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var item = new ToDoItem();

        // Assert
        item.CreatedAtUtc.Should().BeAfter(before);
        item.CreatedAtUtc.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void ToDoItem_WhenCreated_UpdatedAtUtcIsApproximatelyNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var item = new ToDoItem();

        // Assert
        item.UpdatedAtUtc.Should().BeAfter(before);
        item.UpdatedAtUtc.Should().BeBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void ToDoItem_TwoInstances_HaveDifferentIds()
    {
        // Act
        var a = new ToDoItem();
        var b = new ToDoItem();

        // Assert
        a.Id.Should().NotBe(b.Id);
    }
}
