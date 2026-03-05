namespace ToDoBackend.DTOs;

/// <summary>
/// Response payload representing a single to-do item.
/// </summary>
public class ToDoResponse
{
    /// <summary>Gets or sets the unique identifier of the to-do item.</summary>
    public Guid Id { get; set; }

    /// <summary>Gets or sets the title of the to-do item.</summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the optional description or notes.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets a value indicating whether the to-do item is completed.</summary>
    public bool IsCompleted { get; set; }

    /// <summary>Gets or sets the UTC date and time when the item was created.</summary>
    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Gets or sets the UTC date and time when the item was last updated.</summary>
    public DateTime UpdatedAtUtc { get; set; }