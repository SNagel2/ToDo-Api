namespace ToDo_Backend.DTOs;

/// <summary>
/// Data-transfer object returned to the client representing a to-do item.
/// </summary>
public class ToDoResponse
{
    /// <summary>Gets or sets the unique identifier of the to-do item.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the title of the to-do item.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description / notes.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets a value indicating whether the item is completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Gets or sets the UTC creation timestamp.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC last-updated timestamp, or <c>null</c> if never updated.</summary>
    public DateTime? UpdatedAtUtc { get; set; }
}