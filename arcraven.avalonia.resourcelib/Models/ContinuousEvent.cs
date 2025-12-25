namespace Arcraven.Avalonia.ResourcesLib.Models;

/// <summary>
/// Represents an event that spans a duration of time or tracks a fluctuating value 
/// (e.g., Financial rates, Server load, Process cycles).
/// </summary>
public class ContinuousEvent : Event
{
    public ContinuousEvent(
        string name,
        string label,
        Severity severity,
        decimal? value = null,
        Guid? id = null,
        Guid? sourceGuid = null,
        TimeSpan? duration = null,
        bool isOngoing = true,
        string description = "",
        DateTimeOffset? startingTime = null,
        bool isAcknowledged = false)
        : base(name, label, severity, id, duration, isOngoing, sourceGuid, description, startingTime, isAcknowledged)
    {
        Value = value;
    }

    public ContinuousEvent() { }

    

    private decimal? _value;
    /// <summary>
    /// Optional numeric value associated with this continuous event 
    /// (e.g., Stock Price, Temperature, Percentage).
    /// </summary>
    public decimal? Value
    {
        get => _value;
        set => Set(ref _value, value);
    }
}