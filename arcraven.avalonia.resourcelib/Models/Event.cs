using System.Numerics;

namespace Arcraven.Avalonia.ResourcesLib.Models;

/// <summary>
/// Base class representing a generic system event for logging and auditing.
/// Handles Identity, Metadata, Timestamp, and Severity.
/// </summary>
public abstract class Event : ObservableObject
{
    protected Event(
        string name,
        string label,
        Severity severity,
        Guid? id = null,
        TimeSpan? duration = null,
        bool isOngoing = false,
        Guid? sourceGuid = null,
        string description = "",
        DateTimeOffset? startingTime = null,
        bool isAcknowledged = false)
    {
        // Identity
        Id = id ?? Guid.NewGuid();
        SourceGuid = sourceGuid ?? Guid.NewGuid();

        // Metadata
        Name = name;
        Label = label;
        Description = description;

        // Time (Point in time)
        StartingTime = startingTime ?? DateTimeOffset.UtcNow;
        Duration = duration;
        IsOngoing = isOngoing;
        
        // Status
        Severity = severity;
        IsAcknowledged = isAcknowledged;
    }

    /// <summary>
    /// Default constructor for serialization.
    /// </summary>
    protected Event()
    {
        StartingTime = DateTimeOffset.UtcNow;
    }

    // ==========================================
    // Identity
    // ==========================================
    public Guid Id { get; private set; } = Guid.NewGuid();
    protected void SetGuid(Guid guid) => Id = guid;
    public Guid SourceGuid { get; private set; } = Guid.NewGuid();
    protected void SetSourceGuid(Guid guid) => SourceGuid = guid;

    // ==========================================
    // Metadata
    // ==========================================
    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private string _label = string.Empty;
    public string Label
    {
        get => _label;
        set => Set(ref _label, value);
    }

    private string _description = string.Empty;
    public string Description
    {
        get => _description;
        set => Set(ref _description, value);
    }

    // ==========================================
    // Time
    // ==========================================
    private DateTimeOffset _startingTime;
    public DateTimeOffset StartingTime
    {
        get => _startingTime;
        set => Set(ref _startingTime, value);
    }

    private TimeSpan? _duration;
    public TimeSpan? Duration
    {
        get => _duration;
        set => Set(ref _duration, value);
    }

    private bool _isOngoing;
    public bool IsOngoing
    {
        get => _isOngoing;
        set => Set(ref _isOngoing, value);
    }
    
    // ==========================================
    // Status
    // ==========================================
    private Severity _severity = Severity.Indeterminate;
    public Severity Severity
    {
        get => _severity;
        set => Set(ref _severity, value);
    }

    private bool _isAcknowledged = false;
    public bool IsAcknowledged
    {
        get => _isAcknowledged;
        set => Set(ref _isAcknowledged, value);
    }
}