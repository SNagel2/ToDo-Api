using ToDoBackend.Models;

namespace ToDoBackend.Repositories;

/// <summary>
/// Defines the contract for to-do item persistence operations.
/// </summary>
public interface IToDoRepository
{
    /// <summary>
    /// Retrieves all to-do items, with optional status filtering and sort order.
    /// </summary>
    /// <param name="status">The status filter to apply.</param>
    /// <param name="sortOrder">The sort order to apply.</param>
    /// <returns>A collection of <see cref="ToDoItem"/> matching the criteria.</returns>
    Task<IEnumerable<ToDoItem>> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder);

    /// <summary>
    /// Retrieves a single to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <returns>The <see cref="ToDoItem"/> if found; otherwise <c>null</c>.</returns>
    Task<ToDoItem?> GetByIdAsync(Guid id);

    /// <summary>
    /// Persists a new to-do item.
    /// </summary>
    /// <param name="item">The <see cref="ToDoItem"/> to add.</param>
    /// <returns>The newly created <see cref="ToDoItem"/>.</returns>
    Task<ToDoItem> AddAsync(ToDoItem item);

    /// <summary>
    /// Updates an existing to-do item.
    /// </summary>
    /// <param name="item">The <see cref="ToDoItem"/> with updated values.</param>
    /// <returns>The updated <see cref="ToDoItem"/> if it existed; otherwise <c>null</c>.</returns>
    Task<ToDoItem?> UpdateAsync(ToDoItem item);

    /// <summary>
    /// Deletes a to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item to delete.</param>
    /// <returns><c>true</c> if the item was found and deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(Guid id);
}