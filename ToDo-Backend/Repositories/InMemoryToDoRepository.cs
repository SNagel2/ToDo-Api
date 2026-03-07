using System.Collections.Concurrent;
using ToDo_Backend.Interfaces;
using ToDo_Backend.Models;

namespace ToDo_Backend.Repositories;

/// <summary>
/// An in-memory implementation of <see cref="IToDoRepository"/>.
/// All data is stored in a thread-safe in-process collection and is not persisted across restarts.
/// </summary>
public class InMemoryToDoRepository : IToDoRepository
{
    private readonly ConcurrentDictionary<Guid, ToDoItem> _store = new();

    /// <inheritdoc />
    public Task<IReadOnlyList<ToDoItem>> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder)
    {
        IEnumerable<ToDoItem> query = _store.Values;

        query = status switch
        {
            ToDoStatus.Active    => query.Where(t => !t.IsCompleted),
            ToDoStatus.Completed => query.Where(t => t.IsCompleted),
            _                    => query
        };

        query = sortOrder switch
        {
            ToDoSortOrder.OldestFirst => query.OrderBy(t => t.CreatedAtUtc),
            _                         => query.OrderByDescending(t => t.CreatedAtUtc)
        };

        IReadOnlyList<ToDoItem> result = query.ToList();
        return Task.FromResult(result);
    }

    /// <inheritdoc />
    public Task<ToDoItem?> GetByIdAsync(Guid id)
    {
        _store.TryGetValue(id, out var item);
        return Task.FromResult(item);
    }

    /// <inheritdoc />
    public Task<ToDoItem> AddAsync(ToDoItem item)
    {
        _store[item.Id] = item;
        return Task.FromResult(item);
    }

    /// <inheritdoc />
    public Task<ToDoItem?> UpdateAsync(ToDoItem item)
    {
        if (!_store.ContainsKey(item.Id))
            return Task.FromResult<ToDoItem?>(null);

        _store[item.Id] = item;
        return Task.FromResult<ToDoItem?>(item);
    }

    /// <inheritdoc />
    public Task<bool> DeleteAsync(Guid id)
    {
        var removed = _store.TryRemove(id, out _);
        return Task.FromResult(removed);
    }
}