using System.ComponentModel.DataAnnotations;

namespace ToDoBackend.DTOs;

/// <summary>
/// Request payload for updating an existing to-do item.
/// </summary>
public class UpdateToDoRequest
{
    /// <summary>Gets or sets the updated title of the to-do item. Required; must be non-empty after trimming.</summary>
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(1, ErrorMessage = "Title must not be empty.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets the updated optional description or notes.</summary>
    public string? Description { get; set; }

    /// <summary>Gets or sets a value indicating whether the to-do item is completed.</summary>
    public bool IsCompleted { get; set; }