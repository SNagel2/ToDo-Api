using ToDo_Backend.Models;

namespace ToDo_Backend.Interfaces;

/// <summary>
/// Defines data-access operations for <see cref="ToDoItem"/> entities.
/// </summary>
public interface IToDoRepository
{
    /// <summary>
    /// Retrieves all to-do items, optionally filtered by status and sorted.
    /// </summary>
    /// <param name="status">The status filter to apply.</param>
    /// <param name="sortOrder">The sort order to apply.</param>
    /// <returns>A read-only collection of matching <see cref="ToDoItem"/> entities.</returns>
    Task<IReadOnlyList<ToDoItem>> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder);

    /// <summary>
    /// Retrieves a single to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item.</param>
    /// <returns>
    /// The matching <see cref="ToDoItem"/>, or <c>null</c> if no item with that ID exists.
    /// </returns>
    Task<ToDoItem?> GetByIdAsync(Guid id);

    /// <summary>
    /// Persists a new to-do item.
    /// </summary>
    /// <param name="item">The <see cref="ToDoItem"/> to add.</param>
    /// <returns>The newly added <see cref="ToDoItem"/>.</returns>
    Task<ToDoItem> AddAsync(ToDoItem item);

    /// <summary>
    /// Updates an existing to-do item.
    /// </summary>
    /// <param name="item">The <see cref="ToDoItem"/> containing updated values.</param>
    /// <returns>
    /// The updated <see cref="ToDoItem"/>, or <c>null</c> if the item was not found.
    /// </returns>
    Task<ToDoItem?> UpdateAsync(ToDoItem item);

    /// <summary>
    /// Deletes a to-do item by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the to-do item to delete.</param>
    /// <returns><c>true</c> if the item was found and deleted; otherwise <c>false</c>.</returns>
    Task<bool> DeleteAsync(Guid id);
}