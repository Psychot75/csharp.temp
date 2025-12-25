using System.Numerics;

namespace Arcraven.Avalonia.ResourcesLib.Models;

/// <summary>
/// An event tied to a specific physical location in 3D space.
/// </summary>
public class GeoSpatialEvent : Event
{
    public GeoSpatialEvent(
        string name,
        string label,
        Severity severity,
        Vector3 position,
        Guid? id = null,
        Guid? sourceGuid = null,
        TimeSpan? duration = null,
        bool isOngoing = false,
        string description = "",
        DateTimeOffset? startingTime = null,
        bool isAcknowledged = false)
        : base(name, label, severity, id, duration, isOngoing,sourceGuid, description, startingTime, isAcknowledged)
    {
        Position = position;
    }

    public GeoSpatialEvent() { }

    private Vector3 _position;
    
    /// <summary>
    /// Represents 3D coordinates (X=Lat, Y=Lon, Z=Alt or similar convention).
    /// </summary>
    public Vector3 Position
    {
        get => _position;
        set
        {
            if (Set(ref _position, value))
            {
                RaisePropertyChanged(nameof(Latitude), nameof(Longitude), nameof(Altitude));
            }
        }
    }

    public float Latitude => _position.X;
    public float Longitude => _position.Y;
    public float Altitude => _position.Z;
}