using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ToDo_Backend.Controllers;
using ToDo_Backend.DTOs;
using ToDo_Backend.Models;
using ToDo_Backend.Services;
using Xunit;

namespace ToDo_BackendTests.Controllers;

/// <summary>
/// Unit tests for <see cref="ToDosController"/>.
/// All dependencies are mocked via Moq so tests run fully in isolation.
/// </summary>
public class ToDosControllerTests
{
    // -----------------------------------------------------------------------
    // Helpers / test fixtures
    // -----------------------------------------------------------------------

    private readonly Mock<IToDoService> _serviceMock;
    private readonly ToDosController _sut;

    public ToDosControllerTests()
    {
        _serviceMock = new Mock<IToDoService>(MockBehavior.Strict);
        _sut = new ToDosController(_serviceMock.Object);

        // Give the controller a valid HTTP context so ModelState works
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
    }

    private static ToDoResponse MakeResponse(
        string title = "Test",
        bool isCompleted = false,
        string? description = null) => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        Description = description,
        IsCompleted = isCompleted,
        CreatedAtUtc = DateTime.UtcNow,
        UpdatedAtUtc = null
    };

    // -----------------------------------------------------------------------
    // GET /api/todos
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetAll_DefaultParameters_ReturnsOkWithList()
    {
        // Arrange
        var expected = new List<ToDoResponse> { MakeResponse("Item 1"), MakeResponse("Item 2") };
        _serviceMock
            .Setup(s => s.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(expected);

        // Act
        var result = await _sut.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAll_EmptyStore_ReturnsOkWithEmptyList()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(new List<ToDoResponse>());

        // Act
        var result = await _sut.GetAll();

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.Value.Should().BeAssignableTo<IEnumerable<ToDoResponse>>()
            .Which.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ToDoStatus.Active,    ToDoSortOrder.OldestFirst)]
    [InlineData(ToDoStatus.Completed, ToDoSortOrder.NewestFirst)]
    [InlineData(ToDoStatus.All,       ToDoSortOrder.OldestFirst)]
    public async Task GetAll_VariousFilterAndSort_PassesParametersToService(ToDoStatus status, ToDoSortOrder sortOrder)
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetAllAsync(status, sortOrder))
            .ReturnsAsync(new List<ToDoResponse>());

        // Act
        var result = await _sut.GetAll(status, sortOrder);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.GetAllAsync(status, sortOrder), Times.Once);
    }

    // -----------------------------------------------------------------------
    // GET /api/todos/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetById_ExistingId_ReturnsOkWithItem()
    {
        // Arrange
        var response = MakeResponse("My Task");
        _serviceMock
            .Setup(s => s.GetByIdAsync(response.Id))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.GetById(response.Id);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetById_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var missingId = Guid.NewGuid();
        _serviceMock
            .Setup(s => s.GetByIdAsync(missingId))
            .ReturnsAsync((ToDoResponse?)null);

        // Act
        var result = await _sut.GetById(missingId);

        // Assert
        result.Should().BeOfType<NotFoundResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    // -----------------------------------------------------------------------
    // POST /api/todos
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Create_ValidRequest_ReturnsCreatedAtAction()
    {
        // Arrange
        var request  = new CreateToDoRequest { Title = "New task", Description = "Details" };
        var response = MakeResponse(request.Title, description: request.Description);

        _serviceMock
            .Setup(s => s.CreateAsync(request))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.Create(request);

        // Assert
        var created = result.Should().BeOfType<CreatedAtActionResult>().Subject;
        created.StatusCode.Should().Be(StatusCodes.Status201Created);
        created.ActionName.Should().Be(nameof(ToDosController.GetById));
        created.RouteValues.Should().ContainKey("id").WhoseValue.Should().Be(response.Id);
        created.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Create_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange – simulate an empty title (whitespace-only)
        var request = new CreateToDoRequest { Title = "   " };
        // No service call expected

        // Act
        var result = await _sut.Create(request);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Create_InvalidModelState_ReturnsBadRequestWithoutCallingService()
    {
        // Arrange – inject a model-state error manually
        _sut.ModelState.AddModelError(nameof(CreateToDoRequest.Title), "Title is required.");
        var request = new CreateToDoRequest { Title = string.Empty };

        // Act
        var result = await _sut.Create(request);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);

        // Service must never be called when ModelState is invalid
        _serviceMock.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task Create_TitleWithWhitespaceOnly_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateToDoRequest { Title = "\t  \n" };

        // Act
        var result = await _sut.Create(request);

        // Assert – the guard inside the action should catch this
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    // -----------------------------------------------------------------------
    // PUT /api/todos/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Update_ExistingIdAndValidRequest_ReturnsOkWithUpdatedItem()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new UpdateToDoRequest { Title = "Updated", Description = "Desc", IsCompleted = false };
        var response = new ToDoResponse { Id = id, Title = "Updated", Description = "Desc", IsCompleted = false, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };

        _serviceMock
            .Setup(s => s.UpdateAsync(id, request))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.Update(id, request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task Update_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new UpdateToDoRequest { Title = "Updated" };

        _serviceMock
            .Setup(s => s.UpdateAsync(id, request))
            .ReturnsAsync((ToDoResponse?)null);

        // Act
        var result = await _sut.Update(id, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_EmptyTitle_ReturnsBadRequest()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new UpdateToDoRequest { Title = "   " };

        // Act
        var result = await _sut.Update(id, request);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Update_InvalidModelState_ReturnsBadRequestWithoutCallingService()
    {
        // Arrange
        var id = Guid.NewGuid();
        _sut.ModelState.AddModelError(nameof(UpdateToDoRequest.Title), "Title is required.");
        var request = new UpdateToDoRequest { Title = string.Empty };

        // Act
        var result = await _sut.Update(id, request);

        // Assert
        result.Should().BeOfType<ObjectResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        _serviceMock.VerifyNoOtherCalls();
    }

    // -----------------------------------------------------------------------
    // PATCH /api/todos/{id}/status
    // -----------------------------------------------------------------------

    [Fact]
    public async Task PatchStatus_ExistingId_ReturnsOkWithUpdatedItem()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new PatchToDoStatusRequest { IsCompleted = true };
        var response = new ToDoResponse { Id = id, Title = "Task", IsCompleted = true, CreatedAtUtc = DateTime.UtcNow, UpdatedAtUtc = DateTime.UtcNow };

        _serviceMock
            .Setup(s => s.PatchStatusAsync(id, request))
            .ReturnsAsync(response);

        // Act
        var result = await _sut.PatchStatus(id, request);

        // Assert
        var ok = result.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task PatchStatus_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new PatchToDoStatusRequest { IsCompleted = true };

        _serviceMock
            .Setup(s => s.PatchStatusAsync(id, request))
            .ReturnsAsync((ToDoResponse?)null);

        // Act
        var result = await _sut.PatchStatus(id, request);

        // Assert
        result.Should().BeOfType<NotFoundResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task PatchStatus_TogglesCompletionState_ServiceCalledWithCorrectFlag(bool isCompleted)
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = new PatchToDoStatusRequest { IsCompleted = isCompleted };
        var response = new ToDoResponse { Id = id, Title = "Task", IsCompleted = isCompleted, CreatedAtUtc = DateTime.UtcNow };

        _serviceMock
            .Setup(s => s.PatchStatusAsync(id, request))
            .ReturnsAsync(response);

        // Act
        await _sut.PatchStatus(id, request);

        // Assert
        _serviceMock.Verify(s => s.PatchStatusAsync(id, It.Is<PatchToDoStatusRequest>(r => r.IsCompleted == isCompleted)), Times.Once);
    }

    // -----------------------------------------------------------------------
    // DELETE /api/todos/{id}
    // -----------------------------------------------------------------------

    [Fact]
    public async Task Delete_ExistingId_ReturnsNoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        var result = await _sut.Delete(id);

        // Assert
        result.Should().BeOfType<NoContentResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task Delete_NonExistingId_ReturnsNotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        // Act
        var result = await _sut.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundResult>()
            .Which.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }
}
