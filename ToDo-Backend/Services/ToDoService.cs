using ToDoBackend.DTOs;
using ToDoBackend.Models;
using ToDoBackend.Repositories;

namespace ToDoBackend.Services;

/// <summary>
/// Implements business logic for to-do item operations.
/// </summary>
public class ToDoService : IToDoService
{
    private readonly IToDoRepository _repository;

    /// <summary>
    /// Initializes a new instance of <see cref="ToDoService"/>.
    /// </summary>
    /// <param name="repository">The <see cref="IToDoRepository"/> used for data access.</param>
    public ToDoService(IToDoRepository repository)
    {
        _repository = repository;
    }

    /// <inheritdoc />
    public async Task<ToDoListResponse> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder)
    {
        var items = await _repository.GetAllAsync(status, sortOrder);
        var responses = items.Select(MapToResponse).ToList();

        return new ToDoListResponse
        {
            Items      = responses,
            TotalCount = responses.Count
        };
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
        var trimmedTitle = request.Title.Trim();
        if (string.IsNullOrEmpty(trimmedTitle))
            throw new ArgumentException("Title must not be empty.", nameof(request));

        var item = new ToDoItem
        {
            Title       = trimmedTitle,
            Description = request.Description?.Trim()
        };

        var created = await _repository.AddAsync(item);
        return MapToResponse(created);
    }

    /// <inheritdoc />
    /// <remarks>
    /// FIX (B-02 / MEDIUM): The domain object is no longer mutated in-place before the
    /// repository call. A new <see cref="ToDoItem"/> instance is constructed from the
    /// request values and the immutable fields (Id, CreatedAtUtc) are copied from the
    /// retrieved entity. This ensures that if the repository write ever fails (e.g., in a
    /// future database-backed implementation) the original in-store object is not left in a
    /// partially mutated state.
    /// </remarks>
    public async Task<ToDoResponse?> UpdateAsync(Guid id, UpdateToDoRequest request)
    {
        var trimmedTitle = request.Title.Trim();
        if (string.IsNullOrEmpty(trimmedTitle))
            throw new ArgumentException("Title must not be empty.", nameof(request));

        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        // Build a new instance rather than mutating the object currently held in the store.
        // This prevents "phantom success" if a future persistent repository throws on write.
        var updated = new ToDoItem
        {
            Id           = existing.Id,
            Title        = trimmedTitle,
            Description  = request.Description?.Trim(),
            IsCompleted  = request.IsCompleted,
            CreatedAtUtc = existing.CreatedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var result = await _repository.UpdateAsync(updated);
        return result is null ? null : MapToResponse(result);
    }

    /// <inheritdoc />
    /// <remarks>
    /// FIX (B-02 / MEDIUM): Same safe-copy pattern applied as in <see cref="UpdateAsync"/>.
    /// A new <see cref="ToDoItem"/> is created rather than mutating the in-store reference
    /// before the repository call completes.
    /// </remarks>
    public async Task<ToDoResponse?> PatchStatusAsync(Guid id, PatchToDoStatusRequest request)
    {
        var existing = await _repository.GetByIdAsync(id);
        if (existing is null) return null;

        // Build a new instance rather than mutating the object currently held in the store.
        var patched = new ToDoItem
        {
            Id           = existing.Id,
            Title        = existing.Title,
            Description  = existing.Description,
            IsCompleted  = request.IsCompleted,
            CreatedAtUtc = existing.CreatedAtUtc,
            UpdatedAtUtc = DateTime.UtcNow
        };

        var result = await _repository.UpdateAsync(patched);
        return result is null ? null : MapToResponse(result);
    }

    /// <inheritdoc />
    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    // -----------------------------------------------------------------------
    // Private helpers
    // -----------------------------------------------------------------------

    /// <summary>
    /// Maps a <see cref="ToDoItem"/> domain model to a <see cref="ToDoResponse"/> DTO.
    /// </summary>
    /// <param name="item">The source domain model.</param>
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