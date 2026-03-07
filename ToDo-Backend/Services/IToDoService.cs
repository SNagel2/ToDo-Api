using ToDo_Backend.DTOs;
using ToDo_Backend.Models;

namespace ToDo_Backend.Services;

/// <summary>
/// Defines business-logic operations for managing to-do items.
/// </summary>
public interface IToDoService
{
    /// <summary>
    /// Returns all to-do items, applying the requested filter and sort order.
    /// </summary>
    /// <param name="status">Status filter: <see cref="ToDoStatus.All"/>, <see cref="ToDoStatus.Active"/>, or <see cref="ToDoStatus.Completed"/>.</param>
    /// <param name="sortOrder">Sort order: <see cref="ToDoSortOrder.NewestFirst"/> or <see cref="ToDoSortOrder.OldestFirst"/>.</param>
    /// <returns>A read-only list of <see cref="ToDoResponse"/> DTOs.</returns>
    Task<IReadOnlyList<ToDoResponse>> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder);

    /// <summary>
    /// Returns a single to-do item by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <returns>The matching <see cref="ToDoResponse"/>, or <c>null</c> if not found.</returns>
    Task<ToDoResponse?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new to-do item from the supplied request.
    /// </summary>
    /// <param name="request">The <see cref="CreateToDoRequest"/> containing title and optional description.</param>
    /// <returns>The created <see cref="ToDoResponse"/>.</returns>
    Task<ToDoResponse> CreateAsync(CreateToDoRequest request);

    /// <summary>
    /// Fully updates an existing to-do item.
    /// </summary>
    /// <param name="id">The unique identifier of the item to update.</param>
    /// <param name="request">The <see cref="UpdateToDoRequest"/> containing updated values.</param>
    /// <returns>The updated <see cref="ToDoResponse"/>, or <c>null</c> if the item was not found.</returns>
    Task<ToDoResponse?> UpdateAsync(Guid id, UpdateToDoRequest request);

    /// <summary>
    /// Patches only the completion status of a to-do item.
    /// </summary>
    /// <param name="id">The unique identifier of the item to patch.</param>
    /// <param name="request">The <see cref="PatchToDoStatusRequest"/> containing the desired completion state.</param>
    /// <returns>The updated <see cref="ToDoResponse"/>, or <c>null</c> if the item was not found.</returns>
    Task<ToDoResponse?> PatchStatusAsync(Guid id, PatchToDoStatusRequest request);

    /// <summary>
    /// Deletes a to-do item by ID.
    /// </summary>
    /// <param name="id">The unique identifier of the item to delete.</param>
    /// <returns><c>true</c> if deleted successfully; <c>false</c> if not found.</returns>
    Task<bool> DeleteAsync(Guid id);
}