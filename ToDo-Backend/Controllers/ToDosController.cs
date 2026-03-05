using Microsoft.AspNetCore.Mvc;
using ToDoBackend.DTOs;
using ToDoBackend.Models;
using ToDoBackend.Services;

namespace ToDoBackend.Controllers;

/// <summary>
/// RESTful API controller for managing to-do items.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ToDosController : ControllerBase
{
    private readonly IToDoService _service;
    private readonly ILogger<ToDosController> _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="ToDosController"/>.
    /// </summary>
    /// <param name="service">The <see cref="IToDoService"/> used for business logic.</param>
    /// <param name="logger">The <see cref="ILogger{ToDosController}"/> for structured logging.</param>
    public ToDosController(IToDoService service, ILogger<ToDosController> logger)
    {
        _service = service;
        _logger  = logger;
    }

    // -----------------------------------------------------------------------
    // GET api/todos
    // -----------------------------------------------------------------------

    /// <summary>
    /// Retrieves a filtered and sorted list of to-do items.
    /// </summary>
    /// <param name="status">Filter by status: <c>All</c> (default), <c>Active</c>, or <c>Completed</c>.</param>
    /// <param name="sortOrder">Sort order: <c>NewestFirst</c> (default) or <c>OldestFirst</c>.</param>
    /// <returns>A <see cref="ToDoListResponse"/> with the matching items and their total count.</returns>
    /// <response code="200">Returns the list of to-do items.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet]
    [ProducesResponseType(typeof(ToDoListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAll(
        [FromQuery] ToDoStatus    status    = ToDoStatus.All,
        [FromQuery] ToDoSortOrder sortOrder = ToDoSortOrder.NewestFirst)
    {
        try
        {
            var result = await _service.GetAllAsync(status, sortOrder);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving to-do items.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving to-do items." });
        }
    }

    // -----------------------------------------------------------------------
    // GET api/todos/{id}
    // -----------------------------------------------------------------------

    /// <summary>
    /// Retrieves a single to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <returns>The <see cref="ToDoResponse"/> for the requested item.</returns>
    /// <response code="200">Returns the requested to-do item.</response>
    /// <response code="404">No to-do item with the specified <paramref name="id"/> was found.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById([FromRoute] Guid id)
    {
        try
        {
            var item = await _service.GetByIdAsync(id);
            if (item is null)
                return NotFound(new { message = $"To-do item with id '{id}' was not found." });

            return Ok(item);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving to-do item {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while retrieving the to-do item." });
        }
    }

    // -----------------------------------------------------------------------
    // POST api/todos
    // -----------------------------------------------------------------------

    /// <summary>
    /// Creates a new to-do item.
    /// </summary>
    /// <param name="request">The <see cref="CreateToDoRequest"/> payload containing the title and optional description.</param>
    /// <returns>The newly created <see cref="ToDoResponse"/>.</returns>
    /// <response code="201">The to-do item was created successfully.</response>
    /// <response code="400">The request payload is invalid (e.g., empty title).</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPost]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Create([FromBody] CreateToDoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Extra guard: trimmed-title check beyond model validation
        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title must not be empty or whitespace." });

        try
        {
            var created = await _service.CreateAsync(request);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating to-do item.");
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while creating the to-do item." });
        }
    }

    // -----------------------------------------------------------------------
    // PUT api/todos/{id}
    // -----------------------------------------------------------------------

    /// <summary>
    /// Fully updates an existing to-do item.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item to update.</param>
    /// <param name="request">The <see cref="UpdateToDoRequest"/> payload with updated field values.</param>
    /// <returns>The updated <see cref="ToDoResponse"/>.</returns>
    /// <response code="200">The to-do item was updated successfully.</response>
    /// <response code="400">The request payload is invalid (e.g., empty title).</response>
    /// <response code="404">No to-do item with the specified <paramref name="id"/> was found.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdateToDoRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (string.IsNullOrWhiteSpace(request.Title))
            return BadRequest(new { message = "Title must not be empty or whitespace." });

        try
        {
            var updated = await _service.UpdateAsync(id, request);
            if (updated is null)
                return NotFound(new { message = $"To-do item with id '{id}' was not found." });

            return Ok(updated);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating to-do item {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the to-do item." });
        }
    }

    // -----------------------------------------------------------------------
    // PATCH api/todos/{id}/status
    // -----------------------------------------------------------------------

    /// <summary>
    /// Partially updates a to-do item by toggling its completion status.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <param name="request">The <see cref="PatchToDoStatusRequest"/> containing the new completion status.</param>
    /// <returns>The updated <see cref="ToDoResponse"/>.</returns>
    /// <response code="200">The status was updated successfully.</response>
    /// <response code="404">No to-do item with the specified <paramref name="id"/> was found.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(typeof(ToDoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> PatchStatus([FromRoute] Guid id, [FromBody] PatchToDoStatusRequest request)
    {
        try
        {
            var updated = await _service.PatchStatusAsync(id, request);
            if (updated is null)
                return NotFound(new { message = $"To-do item with id '{id}' was not found." });

            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error patching status for to-do item {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while updating the to-do item status." });
        }
    }

    // -----------------------------------------------------------------------
    // DELETE api/todos/{id}
    // -----------------------------------------------------------------------

    /// <summary>
    /// Deletes a to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item to delete.</param>
    /// <returns>No content on success.</returns>
    /// <response code="204">The to-do item was deleted successfully.</response>
    /// <response code="404">No to-do item with the specified <paramref name="id"/> was found.</response>
    /// <response code="500">An unexpected error occurred.</response>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Delete([FromRoute] Guid id)
    {
        try
        {
            var deleted = await _service.DeleteAsync(id);
            if (!deleted)
                return NotFound(new { message = $"To-do item with id '{id}' was not found." });

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting to-do item {Id}.", id);
            return StatusCode(StatusCodes.Status500InternalServerError,
                new { message = "An error occurred while deleting the to-do item." });
        }
    }