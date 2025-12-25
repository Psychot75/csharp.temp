using System;
using Arcraven.Avalonia.HMI;
using Arcraven.Avalonia.ResourcesLib.Logging;
using Avalonia;
using Avalonia.Markup.Xaml.Styling;

namespace Arcraven.Avalonia.HMI;

public static class LocalizationManager
{
    private static readonly ArcLogger _log = ArcLog.ForContext("LocalizationManager");

    public static void Initialize()
    {
        var lang = SettingsService.Current.Language; 
        SetLanguage(lang);
    }

    public static void SetLanguage(string culture)
    {
        try
        {
            var uri = new Uri($"avares://Arcraven.Avalonia.HMI/Assets/i18n/Strings.{culture}.axaml");
            var resourceInclude = new ResourceInclude(uri) { Source = uri };

            if (Application.Current != null)
            {
                var mergedDicts = Application.Current.Resources.MergedDictionaries;
                for (int i = mergedDicts.Count - 1; i >= 0; i--)
                {
                    if (mergedDicts[i] is ResourceInclude include && 
                        include.Source != null && 
                        include.Source.ToString().Contains("/i18n/Strings."))
                    {
                        mergedDicts.RemoveAt(i);
                    }
                }

                mergedDicts.Add(resourceInclude);
            
                _log.Info($"Interface localized to: {culture}");
            }
        }
        catch (Exception ex)
        {
            _log.Error($"Failed to load localization for {culture}.", ex);
        }
    }
}