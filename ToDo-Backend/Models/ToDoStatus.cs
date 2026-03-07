namespace ToDo_Backend.Models;

/// <summary>
/// Defines the filter status options for querying to-do items.
/// </summary>
public enum ToDoStatus
{
    /// <summary>Return all to-do items regardless of completion state.</summary>
    All,

    /// <summary>Return only active (incomplete) to-do items.</summary>
    Active,

    /// <summary>Return only completed to-do items.</summary>
    Completed
}