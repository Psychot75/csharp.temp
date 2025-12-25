using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Arcraven.Avalonia.HMI.Services;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Logging;
using Arcraven.Avalonia.ResourcesLib.ViewModels;
using Avalonia.Threading;
using Duende.IdentityModel.OidcClient;
using ReactiveUI;

namespace Arcraven.Avalonia.HMI.ViewModels;

public class LoginViewModel : ViewModelBase
{
    
    private readonly ArcLogger _log = ArcLog.For<LoginViewModel>();
    
    private readonly AuthService _authService;
    public Action<LoginResult>? OnLoginSuccess { get; set; }

    private string _statusMessage = "Ready for secure login.";
    public string StatusMessage
    {
        get => _statusMessage;
        set => Set(ref _statusMessage, value);
    }
    
    private readonly DispatcherTimer _utcTimer;

    private string _utcTimestamp = DateTime.UtcNow.ToString("HH:mm:ss 'UTC'");
    public string UtcTimestamp
    {
        get => _utcTimestamp;
        private set => Set(ref _utcTimestamp, value);
    }
    
    private string _appVersion = SettingsService.Current.Version;
    public string AppVersion
    {
        get => _appVersion;
        set => Set(ref _appVersion, value);
    }

    private string _projectNumber = SettingsService.Current.ProjectNumber;
    public string ProjectNumber
    {
        get => _projectNumber;
        set => Set(ref _projectNumber, value);
    }

    private bool _hasError;
    public bool HasError
    {
        get => _hasError;
        set => Set(ref _hasError, value);
    }

    public RelayCommand LoginCommand { get; }

    public LoginViewModel(AuthService authService)
    {
        _authService = authService;
        LoginCommand = new RelayCommand(async () => await ExecuteLoginAsync());
        
        _utcTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromSeconds(1)
        };
        _utcTimer.Tick += (_, _) =>
        {
            UtcTimestamp = DateTime.UtcNow.ToString("HH:mm:ss 'UTC'");
        };
        _utcTimer.Start();
    }

    private async Task ExecuteLoginAsync()
    {
        StatusMessage = "Opening system browser...";
        HasError = false;

        // -------------------------------------------------------------------------
        // !! REMOVE BEFORE FLIGHT !!
        // DEBUG BYPASS: Bypasses OIDC/Keycloak handshake for local development.
        // This block must be stripped or disabled before production deployment.
        // -------------------------------------------------------------------------
        #if DEBUG
            _log.Warning("Bypassing Keycloak handshake: Debug flag detected.");
            StatusMessage = "DEBUG MODE: Bypassing authentication...";
            OnLoginSuccess?.Invoke(new LoginResult());
            return; 
        #endif

        try
        {
            var result = await _authService.LoginAsync();

            if (result.IsError)
            {
                HasError = true;
                StatusMessage = $"Auth Error: {result.Error}";
            }
            else
            {
                OnLoginSuccess?.Invoke(result);
            }
        }
        catch (Exception ex)
        {
            HasError = true;
            StatusMessage = "Critical error during browser hand-off.";
            _log.Error("Critical HMI Handshake failure", ex);
        }
    }
}