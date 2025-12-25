namespace Arcraven.Avalonia.ResourcesLib.Models;

public record AuditHardwareInfo(
    string Timestamp,
    string MachineName,
    string OS,
    CpuData Cpu,
    string MotherboardSerial,
    List<RamStick> Ram,
    List<GpuData> Gpus,
    List<DiskData> Disks,
    List<DisplayData> Displays,
    string MachineFingerprint // SHA-256 hash for audit verification
);

public record CpuData(string Model, string ProcessorId, int Cores);
public record RamStick(string Capacity, string Speed, string Slot, string Manufacturer);
public record GpuData(string Name, string DriverVersion, string Memory);
public record DiskData(string Model, string Serial, long SizeGb);
public record DisplayData(string Name, string Resolution, string SerialNumber, double Scaling);