using System.Collections.Concurrent;
using ToDoBackend.Models;

namespace ToDoBackend.Repositories;

/// <summary>
/// An in-memory implementation of <see cref="IToDoRepository"/>.
/// All data is stored in a thread-safe dictionary and is lost when the application restarts.
/// </summary>
public class InMemoryToDoRepository : IToDoRepository
{
    private readonly ConcurrentDictionary<Guid, ToDoItem> _store = new();

    /// <inheritdoc />
    public Task<IEnumerable<ToDoItem>> GetAllAsync(ToDoStatus status, ToDoSortOrder sortOrder)
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

        return Task.FromResult(query);
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