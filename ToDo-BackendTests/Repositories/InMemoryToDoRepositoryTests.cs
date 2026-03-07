using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using ToDo_Backend.Models;
using ToDo_Backend.Repositories;
using Xunit;

namespace ToDo_BackendTests.Repositories;

/// <summary>
/// Unit tests for <see cref="InMemoryToDoRepository"/>.
/// Each test creates a fresh repository instance so tests remain fully isolated.
/// </summary>
public class InMemoryToDoRepositoryTests
{
    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static InMemoryToDoRepository CreateRepo() => new();

    private static ToDoItem MakeItem(
        string title       = "Task",
        bool isCompleted   = false,
        DateTime? createdAt = null) => new()
    {
        Title        = title,
        IsCompleted  = isCompleted,
        CreatedAtUtc = createdAt ?? DateTime.UtcNow
    };

    private static async Task<ToDoItem> SeedAsync(InMemoryToDoRepository repo, ToDoItem? item = null)
    {
        item ??= MakeItem();
        return await repo.AddAsync(item);
    }

    // -----------------------------------------------------------------------
    // AddAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task AddAsync_ValidItem_ReturnsAddedItem()
    {
        // Arrange
        var repo = CreateRepo();
        var item = MakeItem("My Task");

        // Act
        var result = await repo.AddAsync(item);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(item.Id);
        result.Title.Should().Be("My Task");
    }

    [Fact]
    public async Task AddAsync_MultipleItems_StoresAll()
    {
        // Arrange
        var repo = CreateRepo();

        // Act
        await repo.AddAsync(MakeItem("Task A"));
        await repo.AddAsync(MakeItem("Task B"));
        await repo.AddAsync(MakeItem("Task C"));

        var all = await repo.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        all.Should().HaveCount(3);
    }

    // -----------------------------------------------------------------------
    // GetByIdAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsItem()
    {
        // Arrange
        var repo  = CreateRepo();
        var seeded = await SeedAsync(repo);

        // Act
        var result = await repo.GetByIdAsync(seeded.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(seeded.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var repo = CreateRepo();

        // Act
        var result = await repo.GetByIdAsync(Guid.NewGuid());

        // Assert
        result.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // GetAllAsync – filtering
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_StatusAll_ReturnsAllItems()
    {
        // Arrange
        var repo = CreateRepo();
        await repo.AddAsync(MakeItem(isCompleted: false));
        await repo.AddAsync(MakeItem(isCompleted: true));

        // Act
        var result = await repo.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_StatusActive_ReturnsOnlyIncompleteItems()
    {
        // Arrange
        var repo = CreateRepo();
        await repo.AddAsync(MakeItem("Active",    isCompleted: false));
        await repo.AddAsync(MakeItem("Completed", isCompleted: true));

        // Act
        var result = await repo.GetAllAsync(ToDoStatus.Active, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Title.Should().Be("Active");
    }

    [Fact]
    public async Task GetAllAsync_StatusCompleted_ReturnsOnlyCompletedItems()
    {
        // Arrange
        var repo = CreateRepo();
        await repo.AddAsync(MakeItem("Active",    isCompleted: false));
        await repo.AddAsync(MakeItem("Completed", isCompleted: true));

        // Act
        var result = await repo.GetAllAsync(ToDoStatus.Completed, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().HaveCount(1);
        result.Single().Title.Should().Be("Completed");
    }

    [Fact]
    public async Task GetAllAsync_NoItemsMatchFilter_ReturnsEmptyList()
    {
        // Arrange
        var repo = CreateRepo();
        await repo.AddAsync(MakeItem(isCompleted: false));

        // Act  – ask for completed items only
        var result = await repo.GetAllAsync(ToDoStatus.Completed, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().BeEmpty();
    }

    // -----------------------------------------------------------------------
    // GetAllAsync – sorting
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_SortNewestFirst_ReturnsMostRecentItemFirst()
    {
        // Arrange
        var repo  = CreateRepo();
        var older = MakeItem("Older", createdAt: DateTime.UtcNow.AddHours(-2));
        var newer = MakeItem("Newer", createdAt: DateTime.UtcNow.AddHours(-1));

        await repo.AddAsync(older);
        await repo.AddAsync(newer);

        // Act
        var result = await repo.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        result.First().Title.Should().Be("Newer");
        result.Last().Title.Should().Be("Older");
    }

    [Fact]
    public async Task GetAllAsync_SortOldestFirst_ReturnsOldestItemFirst()
    {
        // Arrange
        var repo  = CreateRepo();
        var older = MakeItem("Older", createdAt: DateTime.UtcNow.AddHours(-2));
        var newer = MakeItem("Newer", createdAt: DateTime.UtcNow.AddHours(-1));

        await repo.AddAsync(older);
        await repo.AddAsync(newer);

        // Act
        var result = await repo.GetAllAsync(ToDoStatus.All, ToDoSortOrder.OldestFirst);

        // Assert
        result.First().Title.Should().Be("Older");
        result.Last().Title.Should().Be("Newer");
    }

    // -----------------------------------------------------------------------
    // UpdateAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingItem_ReturnsUpdatedItem()
    {
        // Arrange
        var repo   = CreateRepo();
        var seeded = await SeedAsync(repo, MakeItem("Original"));

        seeded.Title        = "Updated";
        seeded.IsCompleted  = true;
        seeded.UpdatedAtUtc = DateTime.UtcNow;

        // Act
        var result = await repo.UpdateAsync(seeded);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        result.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingItem_ReturnsNull()
    {
        // Arrange
        var repo    = CreateRepo();
        var phantom = MakeItem("Ghost");

        // Act  – never seeded, so it should not be found
        var result = await repo.UpdateAsync(phantom);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_UpdatedItemRetrievableByGetById()
    {
        // Arrange
        var repo   = CreateRepo();
        var seeded = await SeedAsync(repo, MakeItem("Before"));

        seeded.Title = "After";
        await repo.UpdateAsync(seeded);

        // Act
        var fetched = await repo.GetByIdAsync(seeded.Id);

        // Assert
        fetched!.Title.Should().Be("After");
    }

    // -----------------------------------------------------------------------
    // DeleteAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrueAndRemovesItem()
    {
        // Arrange
        var repo   = CreateRepo();
        var seeded = await SeedAsync(repo);

        // Act
        var deleted = await repo.DeleteAsync(seeded.Id);

        // Assert
        deleted.Should().BeTrue();
        var fetched = await repo.GetByIdAsync(seeded.Id);
        fetched.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var repo = CreateRepo();

        // Act
        var result = await repo.DeleteAsync(Guid.NewGuid());

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_DeletedItemNoLongerAppearsInGetAll()
    {
        // Arrange
        var repo = CreateRepo();
        var item = await SeedAsync(repo, MakeItem("ToDelete"));
        await SeedAsync(repo, MakeItem("ToKeep"));

        // Act
        await repo.DeleteAsync(item.Id);
        var all = await repo.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        all.Should().NotContain(i => i.Id == item.Id);
        all.Should().ContainSingle(i => i.Title == "ToKeep");
    }
}
