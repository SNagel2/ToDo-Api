using System.ComponentModel.DataAnnotations;

namespace ToDo_Backend.DTOs;

/// <summary>
/// Data-transfer object used when updating an existing to-do item.
/// </summary>
public class UpdateToDoRequest
{
    /// <summary>
    /// Gets or sets the updated title of the to-do item.
    /// Must be a non-empty, non-whitespace string.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required and cannot be empty.")]
    [MinLength(1, ErrorMessage = "Title must contain at least one character.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the updated description / notes for the to-do item.
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the to-do item is completed.
    /// </summary>
    public bool IsCompleted { get; set; }
}