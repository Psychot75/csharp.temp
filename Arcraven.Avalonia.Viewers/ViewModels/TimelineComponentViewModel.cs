using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Models;
using Arcraven.Avalonia.Viewers.Models;

namespace Arcraven.Avalonia.Viewers.ViewModels;

public partial class TimelineComponentViewModel : ObservableObject, IDisposable
{
    // 1. The "Source of Truth" passed from the Parent
    private readonly ObservableCollection<Event> _sourceEvents;
    public ObservableCollection<TimelineTrack> Tracks { get; } = new();

    // Helper to find existing wrappers quickly (Optimization)
    private readonly Dictionary<Guid, TimelineEvent> _activeWrappers = new();

    public TimelineComponentViewModel(ObservableCollection<Event> sourceEvents)
    {
        _sourceEvents = sourceEvents;

        // A. Load existing data
        foreach (var evt in _sourceEvents)
        {
            AddEventToTimeline(evt);
        }

        // B. Listen for future changes (Add/Remove/Reset)
        _sourceEvents.CollectionChanged += OnSourceCollectionChanged;
    }

    private void OnSourceCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                if (e.NewItems != null)
                    foreach (Event evt in e.NewItems) AddEventToTimeline(evt);
                break;

            case NotifyCollectionChangedAction.Remove:
                if (e.OldItems != null)
                    foreach (Event evt in e.OldItems) RemoveEventFromTimeline(evt);
                break;

            case NotifyCollectionChangedAction.Reset:
                ClearAll();
                break;
                
            // Handle Replace/Move if necessary for your specific logic
        }
    }

    private void AddEventToTimeline(Event evt)
    {
        // 1. Find or Create the Track for this Source
        var track = Tracks.FirstOrDefault(t => t.TrackId == evt.SourceGuid);
        if (track == null)
        {
            track = new TimelineTrack { TrackId = evt.SourceGuid };
            Tracks.Add(track);
        }

        // 2. Create the Wrapper (TimelineEvent)
        // Note: TimelineEvent constructor automatically hooks into PropertyChanged
        var wrapper = new TimelineEvent(evt);
        
        // 3. Add to Track and Cache
        track.Events.Add(wrapper);
        _activeWrappers[evt.Id] = wrapper;
    }

    private void RemoveEventFromTimeline(Event evt)
    {
        if (_activeWrappers.TryGetValue(evt.Id, out var wrapper))
        {
            // 1. Remove from Track
            var track = Tracks.FirstOrDefault(t => t.TrackId == evt.SourceGuid);
            track?.Events.Remove(wrapper);

            // 2. Clean up memory (Unsubscribe events)
            wrapper.Dispose();
            _activeWrappers.Remove(evt.Id);

            // 3. (Optional) Remove track if empty
            if (track != null && track.Events.Count == 0)
            {
                Tracks.Remove(track);
            }
        }
    }

    private void ClearAll()
    {
        foreach (var wrapper in _activeWrappers.Values)
        {
            wrapper.Dispose();
        }
        _activeWrappers.Clear();
        Tracks.Clear();
    }

    public void Dispose()
    {
        // Stop listening to the source when this view model is destroyed
        _sourceEvents.CollectionChanged -= OnSourceCollectionChanged;
        ClearAll();
    }
}