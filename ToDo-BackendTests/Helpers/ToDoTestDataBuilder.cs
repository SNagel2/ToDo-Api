using ToDoBackend.DTOs;
using ToDoBackend.Models;

namespace ToDoBackend.Tests.Helpers;

/// <summary>
/// Provides factory helpers for building test data used across all test classes.
/// </summary>
public static class ToDoTestDataBuilder
{
    /// <summary>Creates a <see cref="ToDoItem"/> with sensible defaults, allowing per-property overrides.</summary>
    public static ToDoItem BuildToDoItem(
        Guid?     id          = null,
        string    title       = "Sample Title",
        string?   description = "Sample description",
        bool      isCompleted = false,
        DateTime? createdAt   = null,
        DateTime? updatedAt   = null)
    {
        var now = DateTime.UtcNow;
        return new ToDoItem
        {
            Id           = id ?? Guid.NewGuid(),
            Title        = title,
            Description  = description,
            IsCompleted  = isCompleted,
            CreatedAtUtc = createdAt ?? now,
            UpdatedAtUtc = updatedAt ?? now
        };
    }

    /// <summary>Creates a <see cref="CreateToDoRequest"/> with sensible defaults.</summary>
    public static CreateToDoRequest BuildCreateRequest(
        string  title       = "New ToDo",
        string? description = "Some notes")
        => new() { Title = title, Description = description };

    /// <summary>Creates an <see cref="UpdateToDoRequest"/> with sensible defaults.</summary>
    public static UpdateToDoRequest BuildUpdateRequest(
        string  title       = "Updated ToDo",
        string? description = "Updated notes",
        bool    isCompleted = false)
        => new() { Title = title, Description = description, IsCompleted = isCompleted };

    /// <summary>Creates a <see cref="PatchToDoStatusRequest"/>.</summary>
    public static PatchToDoStatusRequest BuildPatchStatusRequest(bool isCompleted)
        => new() { IsCompleted = isCompleted };

    /// <summary>Converts a <see cref="ToDoItem"/> to its equivalent <see cref="ToDoResponse"/> DTO.</summary>
    public static ToDoResponse ToResponse(ToDoItem item) => new()
    {
        Id           = item.Id,
        Title        = item.Title,
        Description  = item.Description,
        IsCompleted  = item.IsCompleted,
        CreatedAtUtc = item.CreatedAtUtc,
        UpdatedAtUtc = item.UpdatedAtUtc
    };
}
