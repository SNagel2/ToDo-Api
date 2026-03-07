namespace ToDo_Backend.Models;

/// <summary>
/// Defines the sort order options for querying to-do items.
/// </summary>
public enum ToDoSortOrder
{
    /// <summary>Sort by creation date, newest items first (default).</summary>
    NewestFirst,

    /// <summary>Sort by creation date, oldest items first.</summary>
    OldestFirst
}