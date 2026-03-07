using System;
using FluentAssertions;
using ToDo_Backend.Models;
using Xunit;

namespace ToDo_BackendTests.Models;

/// <summary>
/// Tests for the <see cref="ToDoItem"/> domain model default values.
/// </summary>
public class ToDoItemTests
{
    [Fact]
    public void ToDoItem_NewInstance_HasNonEmptyId()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.Id.Should().NotBeEmpty();
    }

    [Fact]
    public void ToDoItem_NewInstance_IsNotCompletedByDefault()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public void ToDoItem_NewInstance_HasEmptyTitleByDefault()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.Title.Should().BeEmpty();
    }

    [Fact]
    public void ToDoItem_NewInstance_HasNullDescriptionByDefault()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.Description.Should().BeNull();
    }

    [Fact]
    public void ToDoItem_NewInstance_HasNullUpdatedAtUtcByDefault()
    {
        // Act
        var item = new ToDoItem();

        // Assert
        item.UpdatedAtUtc.Should().BeNull();
    }

    [Fact]
    public void ToDoItem_NewInstance_CreatedAtUtcIsCloseToNow()
    {
        // Arrange
        var before = DateTime.UtcNow.AddSeconds(-1);

        // Act
        var item = new ToDoItem();

        // Assert
        item.CreatedAtUtc.Should().BeAfter(before);
        item.CreatedAtUtc.Should().BeOnOrBefore(DateTime.UtcNow.AddSeconds(1));
    }

    [Fact]
    public void ToDoItem_TwoInstances_HaveDifferentIds()
    {
        // Act
        var item1 = new ToDoItem();
        var item2 = new ToDoItem();

        // Assert
        item1.Id.Should().NotBe(item2.Id);
    }
}
