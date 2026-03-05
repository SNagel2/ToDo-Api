using FluentAssertions;
using ToDoBackend.Models;
using ToDoBackend.Repositories;
using ToDoBackend.Tests.Helpers;

namespace ToDoBackend.Tests.Repositories;

/// <summary>
/// Unit tests for <see cref="InMemoryToDoRepository"/>.
/// Each test operates on a fresh repository instance to ensure full isolation.
/// </summary>
public class InMemoryToDoRepositoryTests
{
    private static InMemoryToDoRepository CreateSut() => new();

    // =========================================================================
    // GetAllAsync – filtering
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_WithStatusAll_ReturnsAllItems()
    {
        // Arrange
        var sut = CreateSut();
        await sut.AddAsync(ToDoTestDataBuilder.BuildToDoItem(isCompleted: false));
        await sut.AddAsync(ToDoTestDataBuilder.BuildToDoItem(isCompleted: true));

        // Act
        var result = await sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WithStatusActive_ReturnsOnlyIncompleteItems()
    {
        // Arrange
        var sut = CreateSut();
        await sut.AddAsync(ToDoTestDataBuilder.BuildToDoItem(isCompleted: false));
        await sut.AddAsync(ToDoTestDataBuilder.BuildToDoItem(isCompleted: true));

        // Act
        var result = await sut.GetAllAsync(ToDoStatus.Active, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().HaveCount(1);
        result.Single().IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithStatusCompleted_ReturnsOnlyCompletedItems()
    {
        // Arrange
        var sut = CreateSut();
        await sut.AddAsync(ToDoTestDataBuilder.BuildToDoItem(isCompleted: false));
        await sut.AddAsync(ToDoTestDataBuilder.BuildToDoItem(isCompleted: true));
        await sut.AddAsync(ToDoTestDataBuilder.BuildToDoItem(isCompleted: true));

        // Act
        var result = await sut.GetAllAsync(ToDoStatus.Completed, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(i => i.IsCompleted);
    }

    // =========================================================================
    // GetAllAsync – sorting
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_WithSortOrderNewestFirst_ReturnsMostRecentItemFirst()
    {
        // Arrange
        var sut   = CreateSut();
        var older = ToDoTestDataBuilder.BuildToDoItem(title: "Old", createdAt: DateTime.UtcNow.AddHours(-2));
        var newer = ToDoTestDataBuilder.BuildToDoItem(title: "New", createdAt: DateTime.UtcNow);
        await sut.AddAsync(older);
        await sut.AddAsync(newer);

        // Act
        var result = (await sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst)).ToList();

        // Assert
        result[0].Title.Should().Be("New");
        result[1].Title.Should().Be("Old");
    }

    [Fact]
    public async Task GetAllAsync_WithSortOrderOldestFirst_ReturnsOldestItemFirst()
    {
        // Arrange
        var sut   = CreateSut();
        var older = ToDoTestDataBuilder.BuildToDoItem(title: "Old", createdAt: DateTime.UtcNow.AddHours(-2));
        var newer = ToDoTestDataBuilder.BuildToDoItem(title: "New", createdAt: DateTime.UtcNow);
        await sut.AddAsync(newer);
        await sut.AddAsync(older);

        // Act
        var result = (await sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.OldestFirst)).ToList();

        // Assert
        result[0].Title.Should().Be("Old");
        result[1].Title.Should().Be("New");
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ReturnsEmptyCollection()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().BeEmpty();
    }

    // =========================================================================
    // GetByIdAsync
    // =========================================================================

    [Fact]
    public async Task GetByIdAsync_WhenItemExists_ReturnsItem()
    {
        // Arrange
        var sut  = CreateSut();
        var item = ToDoTestDataBuilder.BuildToDoItem();
        await sut.AddAsync(item);

        // Act
        var result = await sut.GetByIdAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // AddAsync
    // =========================================================================

    [Fact]
    public async Task AddAsync_PersistsItemAndReturnsIt()
    {
        // Arrange
        var sut  = CreateSut();
        var item = ToDoTestDataBuilder.BuildToDoItem(title: "Persist me");

        // Act
        var added = await sut.AddAsync(item);

        // Assert
        added.Should().NotBeNull();
        added.Id.Should().Be(item.Id);
        added.Title.Should().Be("Persist me");

        var retrieved = await sut.GetByIdAsync(item.Id);
        retrieved.Should().NotBeNull();
    }

    [Fact]
    public async Task AddAsync_WithNullDescription_PersistsNullDescription()
    {
        // Arrange
        var sut  = CreateSut();
        var item = ToDoTestDataBuilder.BuildToDoItem(description: null);

        // Act
        var added = await sut.AddAsync(item);

        // Assert
        added.Description.Should().BeNull();
    }

    // =========================================================================
    // UpdateAsync
    // =========================================================================

    [Fact]
    public async Task UpdateAsync_WhenItemExists_UpdatesAndReturnsItem()
    {
        // Arrange
        var sut  = CreateSut();
        var item = ToDoTestDataBuilder.BuildToDoItem(title: "Before");
        await sut.AddAsync(item);

        item.Title       = "After";
        item.IsCompleted = true;

        // Act
        var updated = await sut.UpdateAsync(item);

        // Assert
        updated.Should().NotBeNull();
        updated!.Title.Should().Be("After");
        updated.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_WhenItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        var sut  = CreateSut();
        var item = ToDoTestDataBuilder.BuildToDoItem();

        // Act
        var result = await sut.UpdateAsync(item);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_PersistedChangeIsVisibleOnSubsequentGet()
    {
        // Arrange
        var sut  = CreateSut();
        var item = ToDoTestDataBuilder.BuildToDoItem(title: "Original");
        await sut.AddAsync(item);

        item.Title = "Modified";
        await sut.UpdateAsync(item);

        // Act
        var fetched = await sut.GetByIdAsync(item.Id);

        // Assert
        fetched!.Title.Should().Be("Modified");
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_WhenItemExists_ReturnsTrueAndRemovesItem()
    {
        // Arrange
        var sut  = CreateSut();
        var item = ToDoTestDataBuilder.BuildToDoItem();
        await sut.AddAsync(item);

        // Act
        var deleted = await sut.DeleteAsync(item.Id);

        // Assert
        deleted.Should().BeTrue();
        var fetched = await sut.GetByIdAsync(item.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenItemDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var sut = CreateSut();

        // Act
        var result = await sut.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DoesNotAffectOtherItems()
    {
        // Arrange
        var sut    = CreateSut();
        var keep   = ToDoTestDataBuilder.BuildToDoItem(title: "Keep");
        var remove = ToDoTestDataBuilder.BuildToDoItem(title: "Remove");
        await sut.AddAsync(keep);
        await sut.AddAsync(remove);

        // Act
        await sut.DeleteAsync(remove.Id);

        // Assert
        var remaining = await sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);
        remaining.Should().ContainSingle(i => i.Id == keep.Id);
    }
}
