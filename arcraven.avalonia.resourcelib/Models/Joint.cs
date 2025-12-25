namespace Arcraven.Avalonia.ResourcesLib.Models;

public class Joint : ObservableObject
{
    public Guid Id { get; private set; } = Guid.NewGuid();
    protected void SetGuid(Guid guid) => Id = guid;
    
    private string _name = string.Empty;
    public string Name { get => _name; set => Set(ref _name, value); }

    private double _position;
    public double Position { get => _position; set => Set(ref _position, value); } // In Radians or Degrees

    private double _velocity;
    public double Velocity { get => _velocity; set => Set(ref _velocity, value); }

    private double _load;
    public double Load { get => _load; set => Set(ref _load, value); } // Torque/Current
}