namespace Arcraven.Avalonia.ResourcesLib.Logging;

public static class ArcLog
{
    public static ArcLogger For<T>() => new ArcLogger(typeof(T).Name);
    public static ArcLogger ForContext(string context) => new ArcLogger(context);
}