namespace ToDoBackend.Models;

/// <summary>
/// Represents the filter status options for querying to-do items.
/// </summary>
public enum ToDoStatus
{
    /// <summary>Return all to-do items regardless of completion status.</summary>
    All,

    /// <summary>Return only incomplete (active) to-do items.</summary>
    Active,

    /// <summary>Return only completed to-do items.</summary>
    Completed
}