using ToDoBackend.DTOs;
using ToDoBackend.Models;

namespace ToDoBackend.Services;

/// <summary>
/// Defines the business-logic contract for to-do item operations.
/// </summary>
public interface IToDoService
{
    /// <summary>
    /// Returns a filtered and sorted list of to-do items.
    /// </summary>
    /// <param name="status">The status filter (<see cref="ToDoStatus"/>).</param>
    /// <param name="sortOrder">The sort order (<see cref="ToDoSortOrder"/>).</param>
    /// <returns>A <see cref="ToDoListResponse"/> containing the matching items and total count.</returns>
    Task<ToDoListResponse> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder);

    /// <summary>
    /// Returns a single to-do item by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <returns>A <see cref="ToDoResponse"/> if found; otherwise <c>null</c>.</returns>
    Task<ToDoResponse?> GetByIdAsync(Guid id);

    /// <summary>
    /// Creates a new to-do item from the supplied request data.
    /// </summary>
    /// <param name="request">The <see cref="CreateToDoRequest"/> containing title and optional description.</param>
    /// <returns>The newly created <see cref="ToDoResponse"/>.</returns>
    Task<ToDoResponse> CreateAsync(CreateToDoRequest request);

    /// <summary>
    /// Fully updates an existing to-do item.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item to update.</param>
    /// <param name="request">The <see cref="UpdateToDoRequest"/> with the new field values.</param>
    /// <returns>The updated <see cref="ToDoResponse"/> if found; otherwise <c>null</c>.</returns>
    Task<ToDoResponse?> UpdateAsync(Guid id, UpdateToDoRequest request);

    /// <summary>
    /// Patches only the completion status of a to-do item.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <param name="request">The <see cref="PatchToDoStatusRequest"/> containing the new status.</param>
    /// <returns>The updated <see cref="ToDoResponse"/> if found; otherwise <c>null</c>.</returns>
    Task<ToDoResponse?> PatchStatusAsync(Guid id, PatchToDoStatusRequest request);

    /// <summary>
    /// Deletes a to-do item by identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item to delete.</param>
    /// <returns><c>true</c> if deleted; <c>false</c> if not found.</returns>
    Task<bool> DeleteAsync(Guid id);
}