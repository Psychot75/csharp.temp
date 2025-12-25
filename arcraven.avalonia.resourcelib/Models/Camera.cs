using System.Numerics;

namespace Arcraven.Avalonia.ResourcesLib.Models;

public class Camera : ObservableObject
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    protected void SetGuid(Guid guid) => Id = guid;

    private string _id = string.Empty;

    private string _streamUrl = string.Empty;

    public string StreamUrl
    {
        get => _streamUrl;
        set => Set(ref _streamUrl, value);
    }

    private bool _isActive;

    public bool IsActive
    {
        get => _isActive;
        set => Set(ref _isActive, value);
    }

    private Vector3 _relativePosition;

    public Vector3 RelativePosition
    {
        get => _relativePosition;
        set => Set(ref _relativePosition, value);
    }
}