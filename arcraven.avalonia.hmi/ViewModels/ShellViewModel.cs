using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Arcraven.Avalonia.ResourcesLib;
using Arcraven.Avalonia.ResourcesLib.Models;
using Arcraven.Avalonia.ResourcesLib.ViewModels;
using Avalonia.Threading;

namespace Arcraven.Avalonia.HMI.ViewModels;

public sealed class ShellViewModel : ViewModelBase
{
    public ObservableCollection<PageDefinition> Pages { get; } =
        new ObservableCollection<PageDefinition>();

    private readonly DispatcherTimer _utcTimer;

    private string _utcTimestamp = DateTime.UtcNow.ToString("HH:mm:ss 'UTC'");
    public string UtcTimestamp
    {
        get => _utcTimestamp;
        private set => Set(ref _utcTimestamp, value);
    }

    private double _uiScale;
    public double UiScale
    {
        get => _uiScale;
        set
        {
            if (Set(ref _uiScale, value))
            {
                // Calculate based on parameters from settings
                NavIconSize = SettingsService.Current.BaseNavIconSize * value;
                BaseFontSize = SettingsService.Current.BaseFontSize * value;
                HeadingFontSize = SettingsService.Current.BaseHeadingFontSize * value;
            }
        }
    }

    private double _navIconSize;
    public double NavIconSize
    {
        get => _navIconSize;
        private set => Set(ref _navIconSize, value);
    }

    private double _baseFontSize;
    public double BaseFontSize
    {
        get => _baseFontSize;
        private set => Set(ref _baseFontSize, value);
    }

    private double _headingFontSize;
    public double HeadingFontSize
    {
        get => _headingFontSize;
        private set => Set(ref _headingFontSize, value);
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
    
    private PageDefinition? _selectedPage;
    public PageDefinition? SelectedPage
    {
        get => _selectedPage;
        set
        {
            if (Set(ref _selectedPage, value) && value != null)
            {
                CurrentPage = value.GetInstance(); 
            }
        }
    }

    private ViewModelBase? _currentPage;
    public ViewModelBase? CurrentPage
    {
        get => _currentPage;
        private set => Set(ref _currentPage, value);
    }

    public ShellViewModel()
    {
        var settings = SettingsService.Current;
        var vmBaseType = typeof(ViewModelBase);
        var assembly = typeof(ShellViewModel).Assembly;
        
        UiScale = settings.DefaultUiScale;
        
        var pages = assembly
            .GetTypes()
            .Where(t => !t.IsAbstract && vmBaseType.IsAssignableFrom(t))
            .Select(t => new
            {
                Type = t,
                Attr = t.GetCustomAttribute<ShellPageAttribute>()
            })
            .Where(x => x.Attr != null)
            .OrderBy(x => x.Attr!.Order)
            .ThenBy(x => x.Attr!.Title);

        foreach (var p in pages)
        {
            var attr = p.Attr!;
            Pages.Add(new PageDefinition(
                attr.Key, 
                attr.Title, 
                attr.IconUri, 
                () => (ViewModelBase)Activator.CreateInstance(p.Type)!,
                attr.IsPersistent
            ));
        }

        SelectedPage = Pages.FirstOrDefault();
        
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
}
