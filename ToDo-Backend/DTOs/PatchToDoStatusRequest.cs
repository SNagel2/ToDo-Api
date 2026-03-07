namespace ToDo_Backend.DTOs;

/// <summary>
/// Data-transfer object used when toggling the completion status of a to-do item.
/// </summary>
public class PatchToDoStatusRequest
{
    /// <summary>
    /// Gets or sets the desired completion state of the to-do item.
    /// </summary>
    public bool IsCompleted { get; set; }
}