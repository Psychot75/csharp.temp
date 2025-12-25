namespace Arcraven.Avalonia.ResourcesLib.Models;

/// <summary>
/// Defines the perceived severity of a state or event.
/// Conforms to ISO/IEC 10164-4 (ITU-T X.733) "Perceived Severity".
/// </summary>
public enum Severity
{
    /// <summary>
    /// The severity level cannot be determined.
    /// </summary>
    Indeterminate = 0,

    /// <summary>
    /// The condition represents a potential or impending service-affecting fault
    /// before any significant effects have been felt.
    /// </summary>
    Warning = 1,

    /// <summary>
    /// A non-service-affecting fault condition which requires corrective action
    /// to prevent it from becoming more serious (e.g., redundancy loss).
    /// </summary>
    Minor = 2,

    /// <summary>
    /// A service-affecting condition that requires urgent corrective action.
    /// (Equivalent to standard 'Error').
    /// </summary>
    Major = 3,

    /// <summary>
    /// A service-affecting condition that has occurred and an immediate 
    /// corrective action is required.
    /// </summary>
    Critical = 4,
    
    /// <summary>
    /// The event indicates that a previously reported alarm has been cleared
    /// and functionality is restored. (Equivalent to 'Normal').
    /// </summary>
    Cleared = 5
}