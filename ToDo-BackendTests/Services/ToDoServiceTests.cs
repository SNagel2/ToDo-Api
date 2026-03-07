using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using ToDo_Backend.DTOs;
using ToDo_Backend.Interfaces;
using ToDo_Backend.Models;
using ToDo_Backend.Services;
using Xunit;

namespace ToDo_BackendTests.Services;

/// <summary>
/// Unit tests for <see cref="ToDoService"/>.
/// The <see cref="IToDoRepository"/> is mocked; no real storage is used.
/// </summary>
public class ToDoServiceTests
{
    // -----------------------------------------------------------------------
    // Fixtures
    // -----------------------------------------------------------------------

    private readonly Mock<IToDoRepository> _repoMock;
    private readonly ToDoService _sut;

    public ToDoServiceTests()
    {
        _repoMock = new Mock<IToDoRepository>(MockBehavior.Strict);
        _sut      = new ToDoService(_repoMock.Object);
    }

    private static ToDoItem MakeItem(
        string title       = "Test Task",
        bool isCompleted   = false,
        string? description = null,
        DateTime? createdAt = null) => new()
    {
        Id           = Guid.NewGuid(),
        Title        = title,
        Description  = description,
        IsCompleted  = isCompleted,
        CreatedAtUtc = createdAt ?? DateTime.UtcNow,
        UpdatedAtUtc = null
    };

    // -----------------------------------------------------------------------
    // GetAllAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAllAsync_WithItems_ReturnsMappedResponses()
    {
        // Arrange
        var items = new List<ToDoItem> { MakeItem("A"), MakeItem("B") };
        _repoMock
            .Setup(r => r.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(items);

        // Act
        var result = await _sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().HaveCount(2);
        result.Select(r => r.Title).Should().BeEquivalentTo(new[] { "A", "B" });
    }

    [Fact]
    public async Task GetAllAsync_EmptyRepository_ReturnsEmptyList()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(new List<ToDoItem>());

        // Act
        var result = await _sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ToDoStatus.Active,    ToDoSortOrder.OldestFirst)]
    [InlineData(ToDoStatus.Completed, ToDoSortOrder.NewestFirst)]
    public async Task GetAllAsync_ForwardsFilterAndSortToRepository(ToDoStatus status, ToDoSortOrder sortOrder)
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync(status, sortOrder))
            .ReturnsAsync(new List<ToDoItem>());

        // Act
        await _sut.GetAllAsync(status, sortOrder);

        // Assert
        _repoMock.Verify(r => r.GetAllAsync(status, sortOrder), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_MapsAllFieldsCorrectly()
    {
        // Arrange
        var item = MakeItem("Mapped Task", description: "Some notes");
        item.IsCompleted = true;
        item.UpdatedAtUtc = DateTime.UtcNow.AddMinutes(-1);

        _repoMock
            .Setup(r => r.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(new List<ToDoItem> { item });

        // Act
        var result = await _sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        var dto = result.Single();
        dto.Id.Should().Be(item.Id);
        dto.Title.Should().Be(item.Title);
        dto.Description.Should().Be(item.Description);
        dto.IsCompleted.Should().BeTrue();
        dto.CreatedAtUtc.Should().Be(item.CreatedAtUtc);
        dto.UpdatedAtUtc.Should().Be(item.UpdatedAtUtc);
    }

    // -----------------------------------------------------------------------
    // GetByIdAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetByIdAsync_ExistingId_ReturnsMappedResponse()
    {
        // Arrange
        var item = MakeItem("Existing Task");
        _repoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(item);

        // Act
        var result = await _sut.GetByIdAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
        result.Title.Should().Be(item.Title);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // CreateAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateAsync_ValidRequest_ReturnsCreatedResponse()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "  New Task  ", Description = "  Notes  " };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("New Task",       because: "the service should trim the title");
        result.Description.Should().Be("Notes",    because: "the service should trim the description");
        result.IsCompleted.Should().BeFalse();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_NullDescription_StoresNullDescription()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Task", Description = null };

        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Description.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_TrimsLeadingAndTrailingWhitespace_FromTitle()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "   Padded Title   " };
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Title.Should().Be("Padded Title");
    }

    [Fact]
    public async Task CreateAsync_CallsRepositoryAddOnce()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "Task" };
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        await _sut.CreateAsync(request);

        // Assert
        _repoMock.Verify(r => r.AddAsync(It.IsAny<ToDoItem>()), Times.Once);
    }

    // -----------------------------------------------------------------------
    // UpdateAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task UpdateAsync_ExistingId_ReturnsUpdatedResponse()
    {
        // Arrange
        var existing = MakeItem("Old Title");
        var request  = new UpdateToDoRequest { Title = "  New Title  ", Description = "  Desc  ", IsCompleted = true };

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.UpdateAsync(existing.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("New Title",  because: "title should be trimmed");
        result.Description.Should().Be("Desc",  because: "description should be trimmed");
        result.IsCompleted.Should().BeTrue();
        result.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new UpdateToDoRequest { Title = "Title" };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.UpdateAsync(id, request);

        // Assert
        result.Should().BeNull();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ToDoItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAtUtcToNow()
    {
        // Arrange
        var existing = MakeItem();
        var before   = DateTime.UtcNow.AddSeconds(-1);
        var request  = new UpdateToDoRequest { Title = "New", IsCompleted = false };

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.UpdateAsync(existing.Id, request);

        // Assert
        result!.UpdatedAtUtc.Should().NotBeNull();
        result.UpdatedAtUtc!.Value.Should().BeAfter(before);
    }

    [Fact]
    public async Task UpdateAsync_RepositoryReturnsNull_PropagatesNullToCallers()
    {
        // Arrange — simulate a race condition where the item disappears between GetById and Update
        var existing = MakeItem();
        var request  = new UpdateToDoRequest { Title = "New" };

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>())).ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.UpdateAsync(existing.Id, request);

        // Assert
        result.Should().BeNull();
    }

    // -----------------------------------------------------------------------
    // PatchStatusAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PatchStatusAsync_ExistingId_UpdatesCompletionState()
    {
        // Arrange
        var existing = MakeItem(isCompleted: false);
        var request  = new PatchToDoStatusRequest { IsCompleted = true };

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.PatchStatusAsync(existing.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
        result.UpdatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task PatchStatusAsync_NonExistingId_ReturnsNull()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new PatchToDoStatusRequest { IsCompleted = true };

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.PatchStatusAsync(id, request);

        // Assert
        result.Should().BeNull();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ToDoItem>()), Times.Never);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PatchStatusAsync_SetsIsCompletedCorrectly(bool targetState)
    {
        // Arrange
        var existing = MakeItem(isCompleted: !targetState);
        var request  = new PatchToDoStatusRequest { IsCompleted = targetState };

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.PatchStatusAsync(existing.Id, request);

        // Assert
        result!.IsCompleted.Should().Be(targetState);
    }

    // -----------------------------------------------------------------------
    // DeleteAsync
    // -----------------------------------------------------------------------

    [Fact]
    public async Task DeleteAsync_ExistingId_ReturnsTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingId_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsync_CallsRepositoryDeleteOnce()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        await _sut.DeleteAsync(id);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(id), Times.Once);
    }
}
