using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Cryptography;
using System.Security.Principal;
using System.Text;

namespace Arcraven.Avalonia.ResourcesLib.Helpers;

public static class SecureStorage
{
    private static string? _cachedSeedPath;

    public static string SecureDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "Arcraven", "Security");

    public static string SeedPath 
    {
        get
        {
            if (_cachedSeedPath == null)
            {
                var installPath = AppDomain.CurrentDomain.BaseDirectory;
                
                using var sha = SHA256.Create();
                var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(installPath));
                var uniqueId = BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 12);
                
                _cachedSeedPath = Path.Combine(SecureDirectory, $"audit_{uniqueId}.seed");
            }
            return _cachedSeedPath;
        }
    }

    public static void InitializeSeed()
    {
        try 
        {
            if (!Directory.Exists(SecureDirectory))
            {
                Directory.CreateDirectory(SecureDirectory);
                
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    SecureDirectoryWindows(SecureDirectory);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    SecureDirectoryLinux(SecureDirectory);
                }
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                if (File.Exists(SeedPath))
                {
                    RunChmod(SeedPath, "600");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to secure directory: {ex.Message}");
        }
    }

    [System.Runtime.Versioning.SupportedOSPlatform("windows")]
    private static void SecureDirectoryWindows(string path)
    {
        try 
        {
            var sec = new DirectorySecurity();

            sec.SetAccessRuleProtection(true, false);

            sec.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null),
                FileSystemRights.FullControl,
                AccessControlType.Allow));

            sec.AddAccessRule(new FileSystemAccessRule(
                new SecurityIdentifier(WellKnownSidType.BuiltinAdministratorsSid, null),
                FileSystemRights.FullControl,
                AccessControlType.Allow));

            sec.AddAccessRule(new FileSystemAccessRule(
                WindowsIdentity.GetCurrent().User,
                FileSystemRights.Read | FileSystemRights.Write,
                AccessControlType.Allow));

            new DirectoryInfo(path).SetAccessControl(sec);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] Failed to secure Windows directory: {ex.Message}");
        }
    }
    
    [System.Runtime.Versioning.SupportedOSPlatform("linux")]
    private static void SecureDirectoryLinux(string path)
    {
        try 
        {
            RunChmod(path, "700");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] Failed to secure Linux directory: {ex.Message}");
        }
    }
    
    private static void RunChmod(string path, string permissions)
    {
        using var process = new System.Diagnostics.Process
        {
            StartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "chmod",
                Arguments = $"{permissions} \"{path}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };
    
        process.Start();
        process.WaitForExit();
    
        if (process.ExitCode != 0)
        {
            var error = process.StandardError.ReadToEnd();
            System.Diagnostics.Debug.WriteLine($"chmod failed on {path}: {error}");
        }
    }
}