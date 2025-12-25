using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Models;
using Arcraven.Avalonia.ResourcesLib.ViewModels;

namespace Arcraven.Avalonia.Viewers.ViewModels;

public class MapSurfaceViewModel : ViewModelBase
{
    // --- MAP STATE ---

    private string _centerLabel = "00.00 N, 00.00 E";
    /// <summary>
    /// Text representation of the current map center (for UI overlays).
    /// </summary>
    public string CenterLabel
    {
        get => _centerLabel;
        set => Set(ref _centerLabel, value);
    }

    private double _zoomLevel = 10.0;
    public double ZoomLevel
    {
        get => _zoomLevel;
        set => Set(ref _zoomLevel, value);
    }

    private string _currentLayer = "Tactical";
    public string CurrentLayer
    {
        get => _currentLayer;
        set => Set(ref _currentLayer, value);
    }

    // --- COMMANDS ---

    public RelayCommand ToggleLayerCommand { get; }

    public MapSurfaceViewModel()
    {
        ToggleLayerCommand = new RelayCommand(OnToggleLayer);
    }

    // --- LOGIC ---

    /// <summary>
    /// Called by the parent conductor (GeneralViewModel) when the user selects an event.
    /// Moves the map camera to the target if the event has geospatial data.
    /// </summary>
    public void FocusOnEvent(Event? evt)
    {
        if (evt is GeoSpatialEvent geoEvent)
        {
            // Simulate panning the map to the target
            CenterLabel = $"{geoEvent.Latitude:F4} N, {geoEvent.Longitude:F4} E";
            
            // "Zoom in" effect for specific targets
            ZoomLevel = 16.0; 
        }
        else
        {
            // If selecting a non-spatial event (like a system log), 
            // you might want to remain where you are or zoom out.
        }
    }

    private void OnToggleLayer()
    {
        // Simple cycle between map layers
        if (CurrentLayer == "Tactical")
            CurrentLayer = "Satellite";
        else if (CurrentLayer == "Satellite")
            CurrentLayer = "Hybrid";
        else
            CurrentLayer = "Tactical";
    }
}