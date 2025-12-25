using System.Collections.ObjectModel;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.Viewers.ViewModels;

namespace Arcraven.Avalonia.Viewers.Models;

public class TimelineTrack : ObservableObject
{
    public Guid TrackId { get; set; }
    
    private bool _isMuted = false;
    public bool IsMuted
    {
        get => _isMuted;
        set => Set(ref _isMuted, value);
    }

    private bool _isExpanded = true;
    public bool IsExpanded
    {
        get => _isExpanded;
        set => Set(ref _isExpanded, value);
    }

    private double _height = 40.0;
    public double Height
    {
        get => _height;
        set => Set(ref _height, value);
    }

    public ObservableCollection<TimelineEvent> Events { get; } = new();
}