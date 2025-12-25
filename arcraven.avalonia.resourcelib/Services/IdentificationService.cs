using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using Arcraven.Avalonia.ResourcesLib.Models;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Arcraven.Avalonia.ResourcesLib.Services;

public class IdentificationService 
{
    public AuditHardwareInfo Harvest()
    {
        var isWindows = RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        var info = new AuditHardwareInfo(
            DateTime.UtcNow.ToString("O"),
            Environment.MachineName,
            RuntimeInformation.OSDescription,
            GetCpu(isWindows),
            GetBoardSerial(isWindows),
            GetRam(isWindows),
            GetGpus(isWindows),
            GetDisks(isWindows),
            GetDisplays(),
            ""
        );

        return info with { MachineFingerprint = GenerateFingerprint(info) };
    }

    private CpuData GetCpu(bool win) => win 
        ? new CpuData(RunWinSingle("cpu get name"), RunWinSingle("cpu get processorid"), Environment.ProcessorCount)
        : new CpuData(RunLin("grep -m 1 'model name' /proc/cpuinfo | cut -d: -f2"), RunLin("cat /var/lib/dbus/machine-id"), Environment.ProcessorCount);

    private string GetBoardSerial(bool win) => win 
        ? RunWinSingle("baseboard get serialnumber") 
        : (RunLin("cat /sys/class/dmi/id/board_serial") ?? "Permission Denied");

    private List<RamStick> GetRam(bool win)
    {
        var list = new List<RamStick>();
        if (win) 
        {
            // Get multiple properties. WMIC returns them as a table.
            var lines = RunWinLines("memorychip get Capacity,Speed,DeviceLocator,Manufacturer");
            foreach (var line in lines)
            {
                var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length >= 2)
                {
                    // Basic parsing: Capacity is usually the first large number
                    long.TryParse(parts[0], out long bytes);
                    string gb = $"{(bytes / 1024 / 1024 / 1024)}GB";
                    list.Add(new RamStick(gb, parts.Last() + "MHz", parts.ElementAtOrDefault(1) ?? "Slot", parts.ElementAtOrDefault(2) ?? "Unknown"));
                }
            }
        } 
        else 
        {
            var mem = RunLin("grep MemTotal /proc/meminfo | awk '{print $2}'");
            if (long.TryParse(mem, out long kb))
                list.Add(new RamStick($"{(kb / 1024 / 1024)}GB", "N/A", "System", "N/A"));
        }
        return list;
    }

    private List<GpuData> GetGpus(bool win)
    {
        if (win)
        {
            return RunWinLines("path win32_VideoController get name")
                .Select(name => new GpuData(name, "N/A", "N/A")).ToList();
        }
        return RunLin("lspci | grep VGA").Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(x => new GpuData(x.Trim(), "N/A", "N/A")).ToList();
    }

    private List<DiskData> GetDisks(bool win)
    {
        if (win) {
            var lines = RunWinLines("diskdrive get Model,SerialNumber,Size");
            return lines.Select(l => {
                var p = l.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                return new DiskData(p.FirstOrDefault() ?? "Drive", p.ElementAtOrDefault(1) ?? "Unknown", 0);
            }).ToList();
        }
        return new List<DiskData> { new DiskData("Linux Root", RunLin("lsblk -dno serial /dev/sda") ?? "Unknown", 0) };
    }

    private List<DisplayData> GetDisplays()
    {
        var screens = new List<DisplayData>();
    
        // Direct access via Lifetime - No "GetVisualRoot" needed
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // We use the MainWindow's screen collection
            var screenList = desktop.MainWindow?.Screens?.All;

            if (screenList != null)
            {
                foreach (var s in screenList)
                {
                    screens.Add(new DisplayData(
                        s.DisplayName ?? "Generic Display",
                        $"{s.Bounds.Width}x{s.Bounds.Height}",
                        "N/A",
                        s.Scaling
                    ));
                }
            }
        }
        return screens;
    }

    // --- UTILS ---

    private string GenerateFingerprint(AuditHardwareInfo info)
    {
        // Concatenate core identifiers
        var raw = $"{info.Cpu.ProcessorId}{info.MotherboardSerial}{info.Disks.FirstOrDefault()?.Serial}";
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hashBytes);
    }

    private string RunWinSingle(string args) => RunWinLines(args).FirstOrDefault() ?? "Unknown";

    private List<string> RunWinLines(string args)
    {
        try {
            var psi = new ProcessStartInfo("wmic", args) { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var p = Process.Start(psi);
            var output = p.StandardOutput.ReadToEnd();
            
            return output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line) && !line.Equals("Node", StringComparison.OrdinalIgnoreCase) && !line.Contains("Caption") && !line.Contains("Name") && !line.Contains("SerialNumber")) 
                .ToList();
        } catch { return new List<string>(); }
    }

    private string RunLin(string cmd) {
        try {
            var psi = new ProcessStartInfo("/bin/bash", $"-c \"{cmd}\"") { RedirectStandardOutput = true, UseShellExecute = false, CreateNoWindow = true };
            using var p = Process.Start(psi);
            return p.StandardOutput.ReadToEnd().Trim();
        } catch { return "Error"; }
    }
}