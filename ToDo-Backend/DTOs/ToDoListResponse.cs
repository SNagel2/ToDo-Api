namespace ToDoBackend.DTOs;

/// <summary>
/// Response payload for a paginated/filtered list of to-do items.
/// </summary>
public class ToDoListResponse
{
    /// <summary>Gets or sets the collection of to-do items returned by the query.</summary>
    public IEnumerable<ToDoResponse> Items { get; set; } = Enumerable.Empty<ToDoResponse>();

    /// <summary>Gets or sets the total number of items matching the current filter (before paging).</summary>
    public int TotalCount { get; set; }