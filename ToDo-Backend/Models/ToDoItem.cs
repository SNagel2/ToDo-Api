namespace ToDo_Backend.Models;

/// <summary>
/// Represents a single to-do item in the application.
/// </summary>
public class ToDoItem
{
    /// <summary>
    /// Gets or sets the unique identifier for the to-do item.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Gets or sets the title of the to-do item.
    /// Must be non-empty and will be stored trimmed.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description / notes for the to-do item.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the to-do item has been completed.
    /// </summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>
    /// Gets or sets the UTC date and time when the to-do item was created.
    /// </summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the UTC date and time when the to-do item was last updated.
    /// Null if the item has never been updated after creation.
    /// </summary>
    public DateTime? UpdatedAtUtc { get; set; }
}