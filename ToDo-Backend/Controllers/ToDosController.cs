using Microsoft.AspNetCore.Mvc;
using ToDo_Backend.DTOs;
using ToDo_Backend.Models;
using ToDo_Backend.Services;

namespace ToDo_Backend.Controllers;

/// <summary>
/// RESTful API controller for managing to-do items.
/// Base route: <c>api/todos</c>
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ToDosController : ControllerBase
{
    private readonly IToDoService _service;

    /// <summary>
    /// Initialises a new instance of <see cref="ToDosController"/>.
    /// </summary>
    /// <param name="service">The to-do service used to execute business logic.</param>
    public ToDosController(IToDoService service)
    {
        _service = service;
    }

    // GET api/todos?status=All&sortOrder=NewestFirst
    /// <summary>
    /// Returns a list of to-do items, optionally filtered and sorted.
    /// </summary>
    /// <param name="status">Status filter: All (default), Active, or Completed.</param>
    /// <param name="sortOrder">Sort order: NewestFirst (default) or OldestFirst.</param>
    /// <returns>200 OK with a list of <see cref="ToDoResponse"/> objects.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ToDoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ToDoStatus    status    = ToDoStatus.All,
        [FromQuery] ToDoSortOrder sortOrder = ToDoSortOrder.NewestFirst)
    {
        var items = await _service.GetAllAsync(status, sortOrder);
        return Ok(items);
    }

    // GET api/todos/{id}
    /// <summary>
    /// Returns a single to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <returns>200 OK with the <see cref="ToDoResponse"/>, or 404 Not Found.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        var item = await _service.GetByIdAsync(id);
        return item is null ? NotFound() : Ok(item);
    }

    // POST api/todos
    /// <summary>
    /// Creates a new to-do item.
    /// </summary>
    /// <param name="request">The <see cref="CreateToDoRequest"/> payload.</param>
    /// <returns>201 Created with the new <see cref="ToDoResponse"/>, or 400 Bad Request on validation failure.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateToDoRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        // Guard: title must not be empty after trimming
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required and cannot be empty.");
            return ValidationProblem(ModelState);
        }

        var created = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT api/todos/{id}
    /// <summary>
    /// Fully replaces an existing to-do item.
    /// </summary>
    /// <param name="id">The unique identifier of the item to update.</param>
    /// <param name="request">The <see cref="UpdateToDoRequest"/> payload.</param>
    /// <returns>200 OK with the updated <see cref="ToDoResponse"/>, 400 Bad Request, or 404 Not Found.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateToDoRequest request)
    {
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            ModelState.AddModelError(nameof(request.Title), "Title is required and cannot be empty.");
            return ValidationProblem(ModelState);
        }

        var updated = await _service.UpdateAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    // PATCH api/todos/{id}/status
    /// <summary>
    /// Patches only the completion status of a to-do item (mark complete / incomplete).
    /// </summary>
    /// <param name="id">The unique identifier of the item to patch.</param>
    /// <param name="request">The <see cref="PatchToDoStatusRequest"/> payload.</param>
    /// <returns>200 OK with the updated <see cref="ToDoResponse"/>, or 404 Not Found.</returns>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PatchStatus([FromRoute] Guid id, [FromBody] PatchToDoStatusRequest request)
    {
        var updated = await _service.PatchStatusAsync(id, request);
        return updated is null ? NotFound() : Ok(updated);
    }

    // DELETE api/todos/{id}
    /// <summary>
    /// Deletes a to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the item to delete.</param>
    /// <returns>204 No Content on success, or 404 Not Found.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        var deleted = await _service.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}