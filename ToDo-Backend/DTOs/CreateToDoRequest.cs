using System.ComponentModel.DataAnnotations;

namespace ToDoBackend.DTOs;

/// <summary>
/// Request payload for creating a new to-do item.
/// </summary>
public class CreateToDoRequest
{
    /// <summary>Gets or sets the title of the new to-do item. Required; must be non-empty after trimming.</summary>
    [Required(ErrorMessage = "Title is required.")]
    [MinLength(1, ErrorMessage = "Title must not be empty.")]
    public string Title { get; set; } = string.Empty;

    /// <summary>Gets or sets an optional description or notes for the to-do item.</summary>
    public string? Description { get; set; }