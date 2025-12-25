namespace Arcraven.Avalonia.ResourcesLib.Models;

public class Sensor : ObservableObject
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    protected void SetGuid(Guid guid) => Id = guid;

    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => Set(ref _name, value);
    }

    private string _type = string.Empty;

    public string Type
    {
        get => _type;
        set => Set(ref _type, value);
    } // e.g., "Lidar", "Ultrasonic", "Battery"

    private double _value;

    public double Value
    {
        get => _value;
        set => Set(ref _value, value);
    }

    private string _unit = string.Empty;

    public string Unit
    {
        get => _unit;
        set => Set(ref _unit, value);
    }
}