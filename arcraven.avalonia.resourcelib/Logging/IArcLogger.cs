namespace Arcraven.Avalonia.ResourcesLib.Logging;

public enum LogLevel
{
    Debug,
    Info,
    Warning,
    Error,
    Fatal
}

public interface IArcLogger
{
    void Log(LogLevel level, string message, Exception? ex = null, params object[] args);
    void Debug(string message, params object[] args);
    void Info(string message, params object[] args);
    void Warning(string message, params object[] args);
    void Error(string message, Exception? ex = null, params object[] args);
}