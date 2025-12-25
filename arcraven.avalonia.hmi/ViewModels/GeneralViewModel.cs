using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Models;
using Arcraven.Avalonia.ResourcesLib.ViewModels;
using Arcraven.Avalonia.Viewers.Models;
using Arcraven.Avalonia.Viewers.ViewModels;
using Avalonia.Media;
using Avalonia.Threading;

namespace Arcraven.Avalonia.HMI.ViewModels;

[ShellPage("general", "General", "avares://Arcraven.Avalonia.HMI/Assets/Icons/dashboard_icon.svg", 0, true)]
public class GeneralViewModel : ViewModelBase
{
    // Sub-ViewModels
    public EventListViewModel EventListVm { get; }
    public MapSurfaceViewModel MapVm { get; }
    public TimelineComponentViewModel TimelineVm { get; }
    
    // Visibility State for Layout
    private bool _isSidebarVisible = true;
    public bool IsSidebarVisible
    {
        get => _isSidebarVisible;
        set => Set(ref _isSidebarVisible, value);
    }

    public RelayCommand ToggleSidebarCommand { get; }

    public GeneralViewModel()
    {
        // 1. Create Data
        var events = new ObservableCollection<Event>(); // Load your mock data here

        // 2. Initialize Components
        EventListVm = new EventListViewModel(events);
        MapVm = new MapSurfaceViewModel();
        TimelineVm = new TimelineComponentViewModel(events);

        // 3. Wiring Events (Parent-Child communication)
        EventListVm.SelectionChanged += (s, selectedEvent) => 
        {
            MapVm.FocusOnEvent(selectedEvent);
        };

        ToggleSidebarCommand = new RelayCommand(() => IsSidebarVisible = !IsSidebarVisible);
        
        GenerateMockData(events);
    }
    
    private void GenerateMockData(ObservableCollection<Event> events)
{
    var now = DateTimeOffset.UtcNow;

    // 1. GEO-SPATIAL EVENT (Drone/Location)
    events.Add(new GeoSpatialEvent(
        name: "DroneProximity",
        label: "UAV-04 entered restricted airspace",
        severity: Severity.Warning,
        position: new Vector3(45.4215f, -75.6972f, 120.5f), // Lat, Lon, Alt
        startingTime: now.AddMinutes(-5),
        description: "Geofence breach detected at Sector 7."
    ));

    // 2. CONTINUOUS EVENT (Process/Simulation)
    events.Add(new ContinuousEvent(
        name: "HydroPressure",
        label: "Main Pump Pressure High",
        severity: Severity.Critical,
        duration: TimeSpan.FromMinutes(42),
        value: 8500.5m, // PSI or kPa
        isOngoing: true,
        startingTime: now.AddMinutes(-42),
        description: "Hydraulic pressure exceeding safety thresholds."
    ));

    // 3. RC EVENT - ROBOT 1 (Issues Detected)
    // Create rich data for the Large Card view
    var spotCameras = new List<Camera>
    {
        new Camera { StreamUrl = "ws://127.0.0.1:8443?id=front", IsActive = true, RelativePosition = new Vector3(0.5f, 0, 0) }, // Front
        new Camera { StreamUrl = "ws://127.0.0.1:8443?id=front", IsActive = true, RelativePosition = new Vector3(-0.5f, 0, 0) }, // Rear
        new Camera { StreamUrl = "ws://127.0.0.1:8443?id=front", IsActive = true }
    };

    var spotSensors = new List<Sensor>
    {
        new Sensor { Name = "Lidar", Type = "Lidar", Value = 600, Unit = "rpm" },
        new Sensor { Name = "Battery", Type = "Power", Value = 12.5, Unit = "V" },
        new Sensor { Name = "Temp", Type = "Thermal", Value = 65.0, Unit = "C" }
    };

    var spotJoints = new List<Joint>
    {
        new Joint { Name = "FL_Hip", Position = 0.45, Load = 85 },
        new Joint { Name = "FR_Hip", Position = -0.45, Load = 82 }
    };

    events.Add(new RCEvent(
        name: "SpotConnection",
        label: "Spot Robot Link Lost",
        severity: Severity.Major,
        isControllable: false, // Not controllable
        startingTime: now.AddMinutes(-120),
        description: "Heartbeat lost. Manual takeover available.",
        cameras: spotCameras,
        sensors: spotSensors,
        joints: spotJoints
    ));

    // 4. RC EVENT - ROBOT 2 (Healthy)
    var explorerCameras = new List<Camera>
    {
        new Camera { StreamUrl = "ws://127.0.0.1:8443?id=front", IsActive = true },
        new Camera { StreamUrl = "ws://127.0.0.1:8443?id=front", IsActive = true }
    };

    events.Add(new RCEvent(
        name: "ExplorerUnit",
        label: "Autonomous Patrol Active",
        severity: Severity.Minor, // Healthy state
        isControllable: true,
        startingTime: now.AddMinutes(-10),
        description: "Patrol route alpha proceeding normally.",
        cameras: explorerCameras,
        sensors: new List<Sensor> { new Sensor { Name = "Battery", Value = 98, Unit = "%" } }
    ));
    
    events.Add(new ContinuousEvent(
        name: "DbBackup",
        label: "Daily Archival Routine",
        severity: Severity.Indeterminate,
        duration: TimeSpan.FromMinutes(15),
        isOngoing: false,
        startingTime: now.AddHours(-4),
        value: 100m // 100% complete
    ));
}
}