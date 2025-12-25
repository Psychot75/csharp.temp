using System.Collections.ObjectModel;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Models;
using Arcraven.Avalonia.ResourcesLib.ViewModels;

namespace Arcraven.Avalonia.Viewers.ViewModels;

public class EventListViewModel : ViewModelBase
{
    public ObservableCollection<Event> SystemEvents { get; }
    public Severity[] SeverityOptions { get; } = Enum.GetValues<Severity>();
    
    private bool _isSidebarVisible = true;
    public bool IsSidebarVisible
    {
        get => _isSidebarVisible;
        set => Set(ref _isSidebarVisible, value);
    }
    // --- SELECTION ---
    private Event? _selectedEvent;
    public Event? SelectedEvent
    {
        get => _selectedEvent;
        set
        {
            if (Set(ref _selectedEvent, value))
            {
                SelectionChanged?.Invoke(this, value);
            }
        }
    }

    public event EventHandler<Event?>? SelectionChanged;

    // --- COMMANDS ---
    public RelayCommand<Event> AcknowledgeCommand { get; }
    public RelayCommand<RCEvent> TakeControlCommand { get; }

    public EventListViewModel(ObservableCollection<Event> events)
    {
        SystemEvents = events;
        
        AcknowledgeCommand = new RelayCommand<Event>(OnAcknowledgeEvent);
        TakeControlCommand = new RelayCommand<RCEvent>(OnTakeControl);
    }

    private void OnAcknowledgeEvent(Event? evt)
    {
        if (evt is null) return;
        evt.IsAcknowledged = true;
        evt.Severity = Severity.Cleared;
    }

    private void OnTakeControl(RCEvent? evt)
    {
        if (evt is null) return;
        evt.Label = "Control Acquired";
        evt.Severity = Severity.Minor;
        evt.IsControllable = false;
    }
}