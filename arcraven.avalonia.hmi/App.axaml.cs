using System;
using System.IO;
using System.Linq;
using Arcraven.Avalonia.HMI.Services;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Arcraven.Avalonia.HMI.ViewModels;
using Arcraven.Avalonia.HMI.Views;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Helpers;
using Arcraven.Avalonia.ResourcesLib.Logging;
using Arcraven.Avalonia.ResourcesLib.Models;
using Arcraven.Avalonia.ResourcesLib.Services;
using Avalonia.Controls;

namespace Arcraven.Avalonia.HMI;

public partial class App : Application
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    private readonly ArcLogger _log = ArcLog.For<Application>();
    
    private bool VerifyLogIntegrity()
    {
        var logFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "system.log");
        var seedFile = SecureStorage.SeedPath; 
    
        var validator = new LogValidator();

        if (!validator.VerifyLogFile(logFile, seedFile, out int errorLine))
        {
            if (File.Exists(logFile) && !File.Exists(seedFile))
            {
                 _log.Error("CRITICAL: Secure Audit Seed is missing from restricted storage. Chain of custody broken.");
            }
            else
            {
                 _log.Error($"CRITICAL: Audit log tampering detected at line {errorLine}.");
            }
            return false;
        }

        _log.Info("Audit log integrity verified successfully.");
        return true;
    }
    
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {

            _log.Info("----------------- LAUNCH -----------------");

            if (!VerifyLogIntegrity())
            {
                _log.Info("----------------- ABORTED (INTEGRITY FAILURE) -----------------");
                Environment.Exit(1); 
                return;
            }
            
            var idService = new IdentificationService();
            var hwInfo = idService.Harvest();
            
            LogHardwareAudit(hwInfo); 
            var authService = new AuthService(new SystemBrowser(), SettingsService.Current);
            var loginVm = new LoginViewModel(authService);
            LocalizationManager.Initialize();
            
            var loginWindow = new LoginWindow { DataContext = loginVm };

            loginVm.OnLoginSuccess += (loginResult) =>
            {
                _log.Info("User authenticated successfully on {0}", hwInfo.MachineFingerprint);
                
                var shellVm = new ShellViewModel();
                var mainWindow = new MainWindow { DataContext = shellVm };
                
                if (SettingsService.Current.FullScreenOnDefault)
                    mainWindow.WindowState = WindowState.FullScreen;

                desktop.MainWindow = mainWindow;
                mainWindow.Show();
                loginWindow.Close();
            };

            desktop.MainWindow = loginWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }
    
    private void OnExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        _log.Info($"Application is shutting down. Exit Code: {e.ApplicationExitCode}");
        _log.Info("----------------- END SESSION -----------------");
    }

    private void LogHardwareAudit(AuditHardwareInfo hwInfo)
    {
        _log.Info("--- SYSTEM AUDIT STARTUP ---");
        _log.Info($"Host: {hwInfo.MachineName} | Fingerprint: {hwInfo.MachineFingerprint}");
        _log.Info($"OS: {hwInfo.OS}");
        
        foreach (var ram in hwInfo.Ram)
            _log.Info("RAM Slot: {0} | Capacity: {1} | Speed: {2}", ram.Slot, ram.Capacity, ram.Speed);
            
        _log.Info($"GPU: {string.Join(", ", hwInfo.Gpus.Select(g => g.Name))}");
        
        foreach (var disk in hwInfo.Disks)
            _log.Info($"Storage: {disk.Model} [Serial: {disk.Serial}]");

        foreach (var screen in hwInfo.Displays)
            _log.Info($"Display: {screen.Name} ({screen.Resolution} @ {screen.Scaling}x)");
            
        _log.Info("--- END SYSTEM AUDIT ---");
    }
}