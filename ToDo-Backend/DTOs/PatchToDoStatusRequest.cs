namespace ToDoBackend.DTOs;

/// <summary>
/// Request payload for toggling the completion status of a to-do item.
/// </summary>
public class PatchToDoStatusRequest
{
    /// <summary>Gets or sets a value indicating whether the to-do item should be marked as completed.</summary>
    public bool IsCompleted { get; set; }