using FluentAssertions;
using Moq;
using ToDoBackend.DTOs;
using ToDoBackend.Models;
using ToDoBackend.Repositories;
using ToDoBackend.Services;
using ToDoBackend.Tests.Helpers;

namespace ToDoBackend.Tests.Services;

/// <summary>
/// Unit tests for <see cref="ToDoService"/>.
/// All repository interactions are mocked via Moq.
/// </summary>
public class ToDoServiceTests
{
    private readonly Mock<IToDoRepository> _repoMock;
    private readonly ToDoService           _sut;

    public ToDoServiceTests()
    {
        _repoMock = new Mock<IToDoRepository>(MockBehavior.Strict);
        _sut      = new ToDoService(_repoMock.Object);
    }

    // =========================================================================
    // GetAllAsync
    // =========================================================================

    [Fact]
    public async Task GetAllAsync_WhenItemsExist_ReturnsToDoListResponseWithCorrectCount()
    {
        // Arrange
        var items = new List<ToDoItem>
        {
            ToDoTestDataBuilder.BuildToDoItem(title: "Task A"),
            ToDoTestDataBuilder.BuildToDoItem(title: "Task B", isCompleted: true)
        };
        _repoMock
            .Setup(r => r.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(items);

        // Act
        var result = await _sut.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst);

        // Assert
        result.Should().NotBeNull();
        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoItemsExist_ReturnsEmptyList()
    {
        // Arrange
        _repoMock
            .Setup(r => r.GetAllAsync(ToDoStatus.Active, ToDoSortOrder.OldestFirst))
            .ReturnsAsync(Enumerable.Empty<ToDoItem>());

        // Act
        var result = await _sut.GetAllAsync(ToDoStatus.Active, ToDoSortOrder.OldestFirst);

        // Assert
        result.TotalCount.Should().Be(0);
        result.Items.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_MapsToDoItemFieldsToResponse_Correctly()
    {
        // Arrange
        var item = ToDoTestDataBuilder.BuildToDoItem(title: "Mapped", description: "Desc", isCompleted: true);
        _repoMock
            .Setup(r => r.GetAllAsync(ToDoStatus.Completed, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(new List<ToDoItem> { item });

        // Act
        var result = await _sut.GetAllAsync(ToDoStatus.Completed, ToDoSortOrder.NewestFirst);

        // Assert
        var response = result.Items.Single();
        response.Id.Should().Be(item.Id);
        response.Title.Should().Be(item.Title);
        response.Description.Should().Be(item.Description);
        response.IsCompleted.Should().BeTrue();
        response.CreatedAtUtc.Should().Be(item.CreatedAtUtc);
        response.UpdatedAtUtc.Should().Be(item.UpdatedAtUtc);
    }

    // =========================================================================
    // GetByIdAsync
    // =========================================================================

    [Fact]
    public async Task GetByIdAsync_WhenItemExists_ReturnsToDoResponse()
    {
        // Arrange
        var item = ToDoTestDataBuilder.BuildToDoItem();
        _repoMock.Setup(r => r.GetByIdAsync(item.Id)).ReturnsAsync(item);

        // Act
        var result = await _sut.GetByIdAsync(item.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(item.Id);
    }

    [Fact]
    public async Task GetByIdAsync_WhenItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    // =========================================================================
    // CreateAsync
    // =========================================================================

    [Fact]
    public async Task CreateAsync_WithValidRequest_ReturnsCreatedToDoResponse()
    {
        // Arrange
        var request = ToDoTestDataBuilder.BuildCreateRequest(title: "Buy milk", description: "2% milk");
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.CreateAsync(request);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Buy milk");
        result.Description.Should().Be("2% milk");
        result.IsCompleted.Should().BeFalse();
        result.Id.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateAsync_TrimsWhitespaceFromTitle_BeforePersisting()
    {
        // Arrange
        var request = ToDoTestDataBuilder.BuildCreateRequest(title: "  Trimmed Title  ");
        ToDoItem? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .Callback<ToDoItem>(i => captured = i)
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        await _sut.CreateAsync(request);

        // Assert
        captured!.Title.Should().Be("Trimmed Title");
    }

    [Fact]
    public async Task CreateAsync_TrimsWhitespaceFromDescription_BeforePersisting()
    {
        // Arrange
        var request = ToDoTestDataBuilder.BuildCreateRequest(title: "Title", description: "  spaced  ");
        ToDoItem? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .Callback<ToDoItem>(i => captured = i)
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        await _sut.CreateAsync(request);

        // Assert
        captured!.Description.Should().Be("spaced");
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task CreateAsync_WithEmptyOrWhitespaceTitle_ThrowsArgumentException(string badTitle)
    {
        // Arrange
        var request = ToDoTestDataBuilder.BuildCreateRequest(title: badTitle);

        // Act
        var act = async () => await _sut.CreateAsync(request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Title must not be empty*");
    }

    [Fact]
    public async Task CreateAsync_WithNullDescription_PersistsNullDescription()
    {
        // Arrange
        var request = ToDoTestDataBuilder.BuildCreateRequest(description: null);
        ToDoItem? captured = null;
        _repoMock
            .Setup(r => r.AddAsync(It.IsAny<ToDoItem>()))
            .Callback<ToDoItem>(i => captured = i)
            .ReturnsAsync((ToDoItem item) => item);

        // Act
        await _sut.CreateAsync(request);

        // Assert
        captured!.Description.Should().BeNull();
    }

    // =========================================================================
    // UpdateAsync
    // =========================================================================

    [Fact]
    public async Task UpdateAsync_WhenItemExists_ReturnsUpdatedToDoResponse()
    {
        // Arrange
        var existing = ToDoTestDataBuilder.BuildToDoItem();
        var request  = ToDoTestDataBuilder.BuildUpdateRequest(title: "Updated", isCompleted: true);

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.UpdateAsync(existing.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.Title.Should().Be("Updated");
        result.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAsync_TrimsWhitespaceFromTitle()
    {
        // Arrange
        var existing = ToDoTestDataBuilder.BuildToDoItem();
        var request  = ToDoTestDataBuilder.BuildUpdateRequest(title: "  Padded Title  ");
        ToDoItem? captured = null;

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .Callback<ToDoItem>(i => captured = i)
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        await _sut.UpdateAsync(existing.Id, request);

        // Assert
        captured!.Title.Should().Be("Padded Title");
    }

    [Fact]
    public async Task UpdateAsync_WhenItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildUpdateRequest();

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.UpdateAsync(id, request);

        // Assert
        result.Should().BeNull();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ToDoItem>()), Times.Never);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task UpdateAsync_WithEmptyOrWhitespaceTitle_ThrowsArgumentException(string badTitle)
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildUpdateRequest(title: badTitle);

        // Act
        var act = async () => await _sut.UpdateAsync(id, request);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*Title must not be empty*");
    }

    [Fact]
    public async Task UpdateAsync_WhenRepositoryReturnsNull_ReturnsNull()
    {
        // Arrange
        var existing = ToDoTestDataBuilder.BuildToDoItem();
        var request  = ToDoTestDataBuilder.BuildUpdateRequest();

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.UpdateAsync(existing.Id, request);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsync_SetsUpdatedAtUtc_OnExistingItem()
    {
        // Arrange
        var past = DateTime.UtcNow.AddDays(-1);
        var existing = ToDoTestDataBuilder.BuildToDoItem(updatedAt: past);
        var request  = ToDoTestDataBuilder.BuildUpdateRequest();
        ToDoItem? captured = null;

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .Callback<ToDoItem>(i => captured = i)
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        await _sut.UpdateAsync(existing.Id, request);

        // Assert
        captured!.UpdatedAtUtc.Should().BeAfter(past);
    }

    // =========================================================================
    // PatchStatusAsync
    // =========================================================================

    [Fact]
    public async Task PatchStatusAsync_WhenItemExists_UpdatesIsCompletedToTrue()
    {
        // Arrange
        var existing = ToDoTestDataBuilder.BuildToDoItem(isCompleted: false);
        var request  = ToDoTestDataBuilder.BuildPatchStatusRequest(isCompleted: true);

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.PatchStatusAsync(existing.Id, request);

        // Assert
        result.Should().NotBeNull();
        result!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task PatchStatusAsync_WhenItemExists_UpdatesIsCompletedToFalse()
    {
        // Arrange
        var existing = ToDoTestDataBuilder.BuildToDoItem(isCompleted: true);
        var request  = ToDoTestDataBuilder.BuildPatchStatusRequest(isCompleted: false);

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        var result = await _sut.PatchStatusAsync(existing.Id, request);

        // Assert
        result!.IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task PatchStatusAsync_WhenItemDoesNotExist_ReturnsNull()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildPatchStatusRequest(isCompleted: true);

        _repoMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ToDoItem?)null);

        // Act
        var result = await _sut.PatchStatusAsync(id, request);

        // Assert
        result.Should().BeNull();
        _repoMock.Verify(r => r.UpdateAsync(It.IsAny<ToDoItem>()), Times.Never);
    }

    [Fact]
    public async Task PatchStatusAsync_SetsUpdatedAtUtc_OnExistingItem()
    {
        // Arrange
        var past     = DateTime.UtcNow.AddDays(-1);
        var existing = ToDoTestDataBuilder.BuildToDoItem(isCompleted: false, updatedAt: past);
        var request  = ToDoTestDataBuilder.BuildPatchStatusRequest(isCompleted: true);
        ToDoItem? captured = null;

        _repoMock.Setup(r => r.GetByIdAsync(existing.Id)).ReturnsAsync(existing);
        _repoMock.Setup(r => r.UpdateAsync(It.IsAny<ToDoItem>()))
                 .Callback<ToDoItem>(i => captured = i)
                 .ReturnsAsync((ToDoItem item) => item);

        // Act
        await _sut.PatchStatusAsync(existing.Id, request);

        // Assert
        captured!.UpdatedAtUtc.Should().BeAfter(past);
    }

    // =========================================================================
    // DeleteAsync
    // =========================================================================

    [Fact]
    public async Task DeleteAsync_WhenItemExists_ReturnsTrue()
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
    public async Task DeleteAsync_WhenItemDoesNotExist_ReturnsFalse()
    {
        // Arrange
        var id = Guid.NewGuid();
        _repoMock.Setup(r => r.DeleteAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _sut.DeleteAsync(id);

        // Assert
        result.Should().BeFalse();
    }
}
