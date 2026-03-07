using System.ComponentModel.DataAnnotations;

namespace ToDo_Backend.DTOs;

/// <summary>
/// Data-transfer object used when creating a new to-do item.
/// </summary>
public class CreateToDoRequest
{
    /// <summary>
    /// Gets or sets the title of the new to-do item.
    /// Must be a non-empty, non-whitespace string.
    /// </summary>
    [Required(AllowEmptyStrings = false, ErrorMessage = "Title is required and cannot be empty.")]
    [MinLength(1, ErrorMessage = "Title must contain at least one character.")]
    [MaxLength(200, ErrorMessage = "Title cannot exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional description / notes for the new to-do item.
    /// </summary>
    [MaxLength(2000, ErrorMessage = "Description cannot exceed 2000 characters.")]
    public string? Description { get; set; }
}