namespace Arcraven.Avalonia.Viewers.Models;

public struct GraphPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public object OriginalValue { get; set; } // For tooltips later
}