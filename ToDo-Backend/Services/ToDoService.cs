using ToDo_Backend.DTOs;
using ToDo_Backend.Interfaces;
using ToDo_Backend.Models;

namespace ToDo_Backend.Services;

/// <summary>
/// Default implementation of <see cref="IToDoService"/>.
/// Orchestrates validation and delegates persistence to <see cref="IToDoRepository"/>.
/// </summary>
public class ToDoService : IToDoService
{
    private readonly IToDoRepository _repository;

    /// <summary>
    /// Initialises a new instance of <see cref="ToDoService"/>.
    /// </summary>
    /// <param name="repository">The repository used to persist to-do items.</param>
    public ToDoService(IToDoRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<ToDoResponse>> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder)
    {
        var items = await _repository.GetAllAsync(status, sortOrder);
        return items.Select(MapToResponse).ToList();
    }

    /// <inheritdoc />
    public async Task<ToDoResponse?> GetByIdAsync(Guid id)
    {
        var item = await _repository.GetByIdAsync(id);
        return item is null ? null : MapToResponse(item);
    }

    /// <inheritdoc />
    public async Task<ToDoResponse> CreateAsync(CreateToDoRequest request)
    {
        var item = new ToDoItem
        {
            Title       = request.Title.Trim(),
            Description = request.Description?.Trim()
        };

        var created = await _repository.AddAsync(item);
        return MapToResponse(created);
    }

    /// <inheritdoc />
    public async Task<ToDoResponse?> UpdateAsync(Guid id, UpdateToDoRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.Title        = request.Title.Trim();
        existing.Description  = request.Description?.Trim();
        existing.IsCompleted  = request.IsCompleted;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? null : MapToResponse(updated);
    }

    /// <inheritdoc />
    public async Task<ToDoResponse?> PatchStatusAsync(Guid id, PatchToDoStatusRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        existing.IsCompleted  = request.IsCompleted;
        existing.UpdatedAtUtc = DateTime.UtcNow;

        var updated = await _repository.UpdateAsync(existing);
        return updated is null ? null : MapToResponse(updated);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    // ---------------------------------------------------------------------------
    // Private helpers
    // ---------------------------------------------------------------------------

    /// <summary>
    /// Maps a <see cref="ToDoItem"/> domain model to a <see cref="ToDoResponse"/> DTO.
    /// </summary>
    /// <param name="item">The domain model to map.</param>
    /// <returns>A populated <see cref="ToDoResponse"/>.</returns>
    private static ToDoResponse MapToResponse(ToDoItem item) => new()
    {
        Id           = item.Id,
        Title        = item.Title,
        Description  = item.Description,
        IsCompleted  = item.IsCompleted,
        CreatedAtUtc = item.CreatedAtUtc,
        UpdatedAtUtc = item.UpdatedAtUtc
    };
}