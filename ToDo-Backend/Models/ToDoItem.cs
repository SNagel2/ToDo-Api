namespace ToDoBackend.Models;

/// <summary>
/// Represents a single to-do item in the system.
/// </summary>
public class ToDoItem
{
    /// <summary>Gets or sets the unique identifier of the to-do item.</summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Gets or sets the title of the to-do item.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description/notes for the to-do item.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets a value indicating whether the to-do item is completed.</summary>
    public bool IsCompleted { get; set; } = false;

    /// <summary>Gets or sets the UTC date and time when the item was created.</summary>
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    /// <summary>Gets or sets the UTC date and time when the item was last updated.</summary>
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}