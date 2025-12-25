using System.ComponentModel;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Models;
using Avalonia.Media;

namespace Arcraven.Avalonia.Viewers.Models;

public class TimelineEvent : ObservableObject, IDisposable
{
    public Event Model { get; }

    public TimelineEvent(Event model)
    {
        Model = model;
        UpdateColor();
        Model.PropertyChanged += OnModelPropertyChanged;
    }

    public Guid TrackId => Model.SourceGuid;

    public DateTime StartTime => Model.StartingTime.LocalDateTime;

    public string Name => Model.Name;
    public string Label => Model.Label;

    private Color _color;
    public Color Color
    {
        get => _color;
        set => Set(ref _color, value);
    }
    
    private bool _isVisible = true;
    public bool IsVisible
    {
        get => _isVisible;
        set => Set(ref _isVisible, value);
    }
    
    private void UpdateColor()
    {
        Color = Model.Severity switch
        {
            Severity.Critical => Colors.Red,
            Severity.Major => Colors.Orange,
            Severity.Warning => Colors.Yellow,
            Severity.Indeterminate => Colors.Gray,
            _ => Colors.Blue
        };
    }
    
    private void OnModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Event.Severity))
        {
            UpdateColor();
        }
    }
    
    public void Dispose()
    {
        Model.PropertyChanged -= OnModelPropertyChanged;
    }
}