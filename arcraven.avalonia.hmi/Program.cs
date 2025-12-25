using Avalonia;
using ReactiveUI.Avalonia;
using System;
using System.IO;

namespace Arcraven.Avalonia.HMI;

sealed class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            SettingsService.Load();

            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}