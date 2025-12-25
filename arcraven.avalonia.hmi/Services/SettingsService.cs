using System;
using System.IO;
using System.Text.Json;
using System.Reflection;

namespace Arcraven.Avalonia.HMI;

public class AppSettings
{
    public string Language { get; set; } = "en-GB";
    public bool FullScreenOnDefault { get; set; } = true;
    public string ProjectNumber { get; set; } = "000";
    public string Version { get; set; } = "1.0.0";
    
    public double DefaultUiScale { get; set; } = 1.0;
    public double BaseNavIconSize { get; set; } = 22.0;
    public double BaseFontSize { get; set; } = 13.0;
    public double BaseHeadingFontSize { get; set; } = 17.0;
}

public static class SettingsService
{
    private static readonly string AppName = Assembly.GetExecutingAssembly().GetName().Name ?? "ArcravenApp";

    private static readonly string DefaultFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "appsettings.json");

    private static readonly string UserFolderPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName);
    private static readonly string UserFilePath = Path.Combine(UserFolderPath, "settings.json");

    public static AppSettings Current { get; private set; } = new();

    public static void Load()
    {
        Current = new AppSettings();
        LoadFromFile(DefaultFilePath);
        LoadFromFile(UserFilePath);
    }

    private static void LoadFromFile(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                var loaded = JsonSerializer.Deserialize<AppSettings>(json);
                if (loaded != null)
                {
                    // Update only non-null values or use reflection to merge
                    if (!string.IsNullOrEmpty(loaded.Language)) Current.Language = loaded.Language;
                    Current.FullScreenOnDefault = loaded.FullScreenOnDefault;
                    if (!string.IsNullOrEmpty(loaded.ProjectNumber)) Current.ProjectNumber = loaded.ProjectNumber;
                    if (!string.IsNullOrEmpty(loaded.Version)) Current.Version = loaded.Version;
                }
            }
            catch { /* Log error or ignore corrupted file */ }
        }
    }

    public static void Save()
    {
        try
        {
            Directory.CreateDirectory(UserFolderPath);
            string json = JsonSerializer.Serialize(Current, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(UserFilePath, json);
        }
        catch (Exception) { /* Handle IO errors */ }
    }
}