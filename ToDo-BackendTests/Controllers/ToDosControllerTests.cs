using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using ToDoBackend.Controllers;
using ToDoBackend.DTOs;
using ToDoBackend.Models;
using ToDoBackend.Services;
using ToDoBackend.Tests.Helpers;

namespace ToDoBackend.Tests.Controllers;

/// <summary>
/// Unit tests for <see cref="ToDosController"/>.
/// The <see cref="IToDoService"/> dependency is fully mocked via Moq.
/// </summary>
public class ToDosControllerTests
{
    private readonly Mock<IToDoService>          _serviceMock;
    private readonly Mock<ILogger<ToDosController>> _loggerMock;
    private readonly ToDosController             _sut;

    public ToDosControllerTests()
    {
        _serviceMock = new Mock<IToDoService>(MockBehavior.Strict);
        _loggerMock  = new Mock<ILogger<ToDosController>>();
        _sut         = new ToDosController(_serviceMock.Object, _loggerMock.Object);
    }

    // =========================================================================
    // GET api/todos
    // =========================================================================

    [Fact]
    public async Task GetAll_WhenServiceSucceeds_Returns200OkWithList()
    {
        // Arrange
        var expected = new ToDoListResponse
        {
            Items      = new List<ToDoResponse> { ToDoTestDataBuilder.ToResponse(ToDoTestDataBuilder.BuildToDoItem()) },
            TotalCount = 1
        };
        _serviceMock
            .Setup(s => s.GetAllAsync(ToDoStatus.All, ToDoSortOrder.NewestFirst))
            .ReturnsAsync(expected);

        // Act
        var actionResult = await _sut.GetAll();

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task GetAll_WithFilterAndSort_PassesParametersToService()
    {
        // Arrange
        var expected = new ToDoListResponse { Items = Enumerable.Empty<ToDoResponse>(), TotalCount = 0 };
        _serviceMock
            .Setup(s => s.GetAllAsync(ToDoStatus.Active, ToDoSortOrder.OldestFirst))
            .ReturnsAsync(expected);

        // Act
        var actionResult = await _sut.GetAll(ToDoStatus.Active, ToDoSortOrder.OldestFirst);

        // Assert
        actionResult.Should().BeOfType<OkObjectResult>();
        _serviceMock.Verify(s => s.GetAllAsync(ToDoStatus.Active, ToDoSortOrder.OldestFirst), Times.Once);
    }

    [Fact]
    public async Task GetAll_WhenServiceThrows_Returns500InternalServerError()
    {
        // Arrange
        _serviceMock
            .Setup(s => s.GetAllAsync(It.IsAny<ToDoStatus>(), It.IsAny<ToDoSortOrder>()))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        var actionResult = await _sut.GetAll();

        // Assert
        var result = actionResult.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // =========================================================================
    // GET api/todos/{id}
    // =========================================================================

    [Fact]
    public async Task GetById_WhenItemExists_Returns200OkWithItem()
    {
        // Arrange
        var item     = ToDoTestDataBuilder.BuildToDoItem();
        var response = ToDoTestDataBuilder.ToResponse(item);
        _serviceMock.Setup(s => s.GetByIdAsync(item.Id)).ReturnsAsync(response);

        // Act
        var actionResult = await _sut.GetById(item.Id);

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task GetById_WhenItemDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ReturnsAsync((ToDoResponse?)null);

        // Act
        var actionResult = await _sut.GetById(id);

        // Assert
        var notFound = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task GetById_WhenServiceThrows_Returns500InternalServerError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.GetByIdAsync(id)).ThrowsAsync(new Exception("DB error"));

        // Act
        var actionResult = await _sut.GetById(id);

        // Assert
        var result = actionResult.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // =========================================================================
    // POST api/todos
    // =========================================================================

    [Fact]
    public async Task Create_WithValidRequest_Returns201CreatedWithItem()
    {
        // Arrange
        var request  = ToDoTestDataBuilder.BuildCreateRequest();
        var created  = ToDoTestDataBuilder.ToResponse(ToDoTestDataBuilder.BuildToDoItem(title: request.Title));
        _serviceMock.Setup(s => s.CreateAsync(request)).ReturnsAsync(created);

        // Act
        var actionResult = await _sut.Create(request);

        // Assert
        var createdAt = actionResult.Should().BeOfType<CreatedAtActionResult>().Subject;
        createdAt.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdAt.Value.Should().BeEquivalentTo(created);
        createdAt.ActionName.Should().Be(nameof(ToDosController.GetById));
    }

    [Fact]
    public async Task Create_WithWhitespaceTitleAfterModelValidation_Returns400BadRequest()
    {
        // Arrange – simulate the extra guard in the controller
        var request = ToDoTestDataBuilder.BuildCreateRequest(title: "   ");

        // Act
        var actionResult = await _sut.Create(request);

        // Assert
        var badRequest = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Create_WhenServiceThrowsArgumentException_Returns400BadRequest()
    {
        // Arrange
        var request = ToDoTestDataBuilder.BuildCreateRequest();
        _serviceMock
            .Setup(s => s.CreateAsync(request))
            .ThrowsAsync(new ArgumentException("Title must not be empty."));

        // Act
        var actionResult = await _sut.Create(request);

        // Assert
        var badRequest = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Create_WhenServiceThrowsUnexpectedException_Returns500InternalServerError()
    {
        // Arrange
        var request = ToDoTestDataBuilder.BuildCreateRequest();
        _serviceMock
            .Setup(s => s.CreateAsync(request))
            .ThrowsAsync(new Exception("Unexpected failure"));

        // Act
        var actionResult = await _sut.Create(request);

        // Assert
        var result = actionResult.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // =========================================================================
    // PUT api/todos/{id}
    // =========================================================================

    [Fact]
    public async Task Update_WithValidRequest_Returns200OkWithUpdatedItem()
    {
        // Arrange
        var id       = Guid.NewGuid();
        var request  = ToDoTestDataBuilder.BuildUpdateRequest();
        var updated  = ToDoTestDataBuilder.ToResponse(ToDoTestDataBuilder.BuildToDoItem(id: id, title: request.Title));
        _serviceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync(updated);

        // Act
        var actionResult = await _sut.Update(id, request);

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(updated);
    }

    [Fact]
    public async Task Update_WithWhitespaceTitleAfterModelValidation_Returns400BadRequest()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildUpdateRequest(title: "   ");

        // Act
        var actionResult = await _sut.Update(id, request);

        // Assert
        var badRequest = actionResult.Should().BeOfType<BadRequestObjectResult>().Subject;
        badRequest.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task Update_WhenItemDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildUpdateRequest();
        _serviceMock.Setup(s => s.UpdateAsync(id, request)).ReturnsAsync((ToDoResponse?)null);

        // Act
        var actionResult = await _sut.Update(id, request);

        // Assert
        var notFound = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Update_WhenServiceThrowsArgumentException_Returns400BadRequest()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildUpdateRequest();
        _serviceMock
            .Setup(s => s.UpdateAsync(id, request))
            .ThrowsAsync(new ArgumentException("Title must not be empty."));

        // Act
        var actionResult = await _sut.Update(id, request);

        // Assert
        actionResult.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Update_WhenServiceThrowsUnexpectedException_Returns500InternalServerError()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildUpdateRequest();
        _serviceMock
            .Setup(s => s.UpdateAsync(id, request))
            .ThrowsAsync(new Exception("Unexpected"));

        // Act
        var actionResult = await _sut.Update(id, request);

        // Assert
        var result = actionResult.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // =========================================================================
    // PATCH api/todos/{id}/status
    // =========================================================================

    [Fact]
    public async Task PatchStatus_WhenItemExists_Returns200OkWithUpdatedItem()
    {
        // Arrange
        var id       = Guid.NewGuid();
        var request  = ToDoTestDataBuilder.BuildPatchStatusRequest(isCompleted: true);
        var response = ToDoTestDataBuilder.ToResponse(ToDoTestDataBuilder.BuildToDoItem(id: id, isCompleted: true));
        _serviceMock.Setup(s => s.PatchStatusAsync(id, request)).ReturnsAsync(response);

        // Act
        var actionResult = await _sut.PatchStatus(id, request);

        // Assert
        var ok = actionResult.Should().BeOfType<OkObjectResult>().Subject;
        ok.StatusCode.Should().Be(StatusCodes.Status200OK);
        ok.Value.Should().BeEquivalentTo(response);
    }

    [Fact]
    public async Task PatchStatus_WhenItemDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildPatchStatusRequest(isCompleted: true);
        _serviceMock.Setup(s => s.PatchStatusAsync(id, request)).ReturnsAsync((ToDoResponse?)null);

        // Act
        var actionResult = await _sut.PatchStatus(id, request);

        // Assert
        actionResult.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task PatchStatus_WhenServiceThrows_Returns500InternalServerError()
    {
        // Arrange
        var id      = Guid.NewGuid();
        var request = ToDoTestDataBuilder.BuildPatchStatusRequest(isCompleted: true);
        _serviceMock
            .Setup(s => s.PatchStatusAsync(id, request))
            .ThrowsAsync(new Exception("Failure"));

        // Act
        var actionResult = await _sut.PatchStatus(id, request);

        // Assert
        var result = actionResult.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    // =========================================================================
    // DELETE api/todos/{id}
    // =========================================================================

    [Fact]
    public async Task Delete_WhenItemExists_Returns204NoContent()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(true);

        // Act
        var actionResult = await _sut.Delete(id);

        // Assert
        var noContent = actionResult.Should().BeOfType<NoContentResult>().Subject;
        noContent.StatusCode.Should().Be(StatusCodes.Status204NoContent);
    }

    [Fact]
    public async Task Delete_WhenItemDoesNotExist_Returns404NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).ReturnsAsync(false);

        // Act
        var actionResult = await _sut.Delete(id);

        // Assert
        var notFound = actionResult.Should().BeOfType<NotFoundObjectResult>().Subject;
        notFound.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task Delete_WhenServiceThrows_Returns500InternalServerError()
    {
        // Arrange
        var id = Guid.NewGuid();
        _serviceMock.Setup(s => s.DeleteAsync(id)).ThrowsAsync(new Exception("Storage failure"));

        // Act
        var actionResult = await _sut.Delete(id);

        // Assert
        var result = actionResult.Should().BeOfType<ObjectResult>().Subject;
        result.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }
}
