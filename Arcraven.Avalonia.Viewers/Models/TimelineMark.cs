using Arcraven.Avalonia.ResourcesLib;
using Avalonia.Media;

namespace Arcraven.Avalonia.Viewers.Models;

public class TimelineMark : ObservableObject
{
    public Guid Guid { get; } = Guid.NewGuid();
    
    private string _label = string.Empty;
    public string Label { 
        get => _label;
        set => Set(ref _label, value);
    }

    private DateTime _position;
    public DateTime Position
    {
        get => _position;
        set => Set(ref _position, value);
    }
    
    private Color _color;
    public Color Color
    {
        get => _color;
        set => Set(ref _color, value);
    }
}
