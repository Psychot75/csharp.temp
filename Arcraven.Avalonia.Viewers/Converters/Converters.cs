using System.Globalization;
using Arcraven.Avalonia.ResourcesLib.Models;
using Arcraven.Avalonia.Viewers.ViewModels;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace Arcraven.Avalonia.Viewers.Converters;
// Converts a Duration (TimeSpan) to Pixel Width
public class DurationToWidthConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 2 && 
            values[0] is TimeSpan duration && 
            values[1] is double pixelsPerSecond)
        {
            return duration.TotalSeconds * pixelsPerSecond;
        }
        return 0.0;
    }
}


// Converts a StartTime (DateTime) to Pixel Left Position relative to ViewStartTime
public class TimeToPositionConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Count == 3 && 
            values[0] is DateTime eventTime && 
            values[1] is DateTime viewStartTime && 
            values[2] is double pixelsPerSecond)
        {
            var offset = eventTime - viewStartTime;
            return offset.TotalSeconds * pixelsPerSecond;
        }
        return 0.0;
    }
}

public class WidthToTemplateConverter : IValueConverter
{
    public IDataTemplate SmallTemplate { get; set; }
    public IDataTemplate MediumTemplate { get; set; }
    public IDataTemplate LargeTemplate { get; set; }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is double width)
        {
            if (width < 300) return SmallTemplate;
            if (width > 480) return LargeTemplate;
            return MediumTemplate;
        }
        return MediumTemplate;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => null;
}

public class SeverityToIconConverter : IValueConverter
{
    // ---------------------------------------------------------
    // GEOMETRIES (Modern, Slim, Outline Style)
    // ---------------------------------------------------------

    // Error: Slim Octagon with Exclamation
    // Represents a "stop" or "critical" failure without being bulky.
    private static readonly Geometry ErrorGeo = Geometry.Parse(
        "M15.73,3H8.27L3,8.27V15.73L8.27,21H15.73L21,15.73V8.27L15.73,3M19,14.9L14.9,19H9.1L5,14.9V9.1L9.1,5H14.9L19,9.1V14.9M11,7H13V13H11V7M11,15H13V17H11V15");

    // Warning: Slim Rounded Triangle
    // Classic warning shape but with a hollow center and thinner strokes.
    private static readonly Geometry WarningGeo = Geometry.Parse(
        "M12.87,1.51L23.59,19.51C24.14,20.44 23.49,21.5 22.5,21.5H1.5C0.51,21.5 -0.14,20.44 0.41,19.51L11.13,1.51C11.59,0.73 12.41,0.73 12.87,1.51M12,4.5L3.5,19.5H20.5L12,4.5M11,10V14H13V10M11,16V18H13V16");

    // Info: Slim Circle with 'i'
    // Clean ring with negative space for the text.
    private static readonly Geometry InfoGeo = Geometry.Parse(
        "M11,9H13V7H11M12,20C7.59,20 4,16.41 4,12C4,7.59 7.59,4 12,4C16.41,4 20,7.59 20,12C20,16.41 16.41,20 12,20M12,2A10,10 0 0,0 2,12A10,10 0 0,0 12,22A10,10 0 0,0 22,12A10,10 0 0,0 12,2M11,17H13V11H11V17Z");

    // Cleared/Good: Slim Circle with Check
    // A hollow ring with a thin, sharp checkmark inside.
    private static readonly Geometry CheckGeo = Geometry.Parse(
        "M12,2A10,10 0 1,0 22,12A10,10 0 0,0 12,2M12,20A8,8 0 1,1 20,12A8,8 0 0,1 12,20M16.59,7.58L10,14.17L7.41,11.59L6,13L10,17L18,9L16.59,7.58Z");


    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not Severity severity) return null;

        // 1. Return a Brush (Color)
        if (targetType == typeof(IBrush))
        {
            return severity switch
            {
                Severity.Critical or Severity.Major => Application.Current?.FindResource("Arc.StatusErrorBrush"),
                Severity.Warning => Application.Current?.FindResource("Arc.StatusWarningBrush"),
                Severity.Cleared => Application.Current?.FindResource("Arc.StatusOnlineBrush"),
                Severity.Minor => Application.Current?.FindResource("Arc.AccentPrimaryBrush"),
                _ => Application.Current?.FindResource("Arc.AccentSecondaryBrush")
            };
        }

        // 2. Return a Geometry (Icon Shape)
        if (targetType == typeof(Geometry))
        {
            return severity switch
            {
                Severity.Critical or Severity.Major => ErrorGeo,
                Severity.Warning => WarningGeo,
                Severity.Cleared => CheckGeo,
                _ => InfoGeo
            };
        }

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}