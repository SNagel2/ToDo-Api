namespace ToDoBackend.Models;

/// <summary>
/// Represents the available sort orders for to-do item lists.
/// </summary>
public enum ToDoSortOrder
{
    /// <summary>Sort by creation date descending (newest first).</summary>
    NewestFirst,

    /// <summary>Sort by creation date ascending (oldest first).</summary>
    OldestFirst
}