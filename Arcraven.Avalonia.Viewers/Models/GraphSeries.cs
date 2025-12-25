using Arcraven.Avalonia.ResourcesLib;
using Avalonia.Media;

namespace Arcraven.Avalonia.Viewers.Models;

public class GraphSeries : ObservableObject
{
    private string _title = string.Empty;
    public string Title 
    { 
        get => _title; 
        set => Set(ref _title, value); 
    }

    private Color _color = Colors.White;
    public Color Color 
    { 
        get => _color; 
        set => Set(ref _color, value); 
    }

    private List<GraphPoint> _points = new();
    public List<GraphPoint> Points 
    { 
        get => _points; 
        set => Set(ref _points, value); 
    }

    private bool _isCurved = true;
    public bool IsCurved 
    { 
        get => _isCurved; 
        set => Set(ref _isCurved, value); 
    }
}