using System.Text.RegularExpressions;

namespace Arcraven.Avalonia.ResourcesLib.Logging;

public class ArcLogger : IArcLogger
{
    private readonly string _context;

    private static readonly string LogDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
    private static readonly string LogFilePath = Path.Combine(LogDir, "system.log");
    
    private static readonly object _lock = new();
    private static string _lastHash = string.Empty;
    private static bool _isInitialized = false;

    private static LogCryptoProvider _crypto;

    public ArcLogger(string context)
    {
        _context = context;
        InitializeChain();
    }

    private void InitializeChain()
    {
        if (_isInitialized) return;

        lock (_lock)
        {
            if (_isInitialized) return;

            if (!Directory.Exists(LogDir)) Directory.CreateDirectory(LogDir);

            _crypto = new LogCryptoProvider();

            if (File.Exists(LogFilePath))
            {
                _lastHash = GetLastHashFromLog();
            }
            else
            {
                _lastHash = _crypto.GetGenesisHash();
            }

            _isInitialized = true;
        }
    }
    
    private string GetLastHashFromLog()
    {
        try
        {
            var lastLine = File.ReadLines(LogFilePath).LastOrDefault();
            if (string.IsNullOrWhiteSpace(lastLine)) return _crypto.GetGenesisHash();

            var match = Regex.Match(lastLine, @"\[(?<hash>[A-Fa-f0-9]{16})\]");
            return match.Success ? match.Groups["hash"].Value : _crypto.GetGenesisHash();
        }
        catch 
        { 
            return _crypto.GetGenesisHash(); 
        }
    }
    
    public void Log(LogLevel level, string message, Exception? ex = null, params object[] args) 
        => WriteLog(level.ToString().ToUpper(), args.Length > 0 ? string.Format(message, args) : message, ex);
        
    public void Debug(string message, params object[] args) => WriteLog("DEBUG", args.Length > 0 ? string.Format(message, args) : message);
    public void Info(string message, params object[] args) => WriteLog("INFO", args.Length > 0 ? string.Format(message, args) : message);
    public void Warning(string message, params object[] args) => WriteLog("WARN", args.Length > 0 ? string.Format(message, args) : message);
    public void Error(string message, Exception? ex = null, params object[] args) 
        => WriteLog("ERROR", args.Length > 0 ? string.Format(message, args) : message, ex);

    private void WriteLog(string level, string message, Exception? ex = null)
    {
        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ");

        string cleanMessage = message.Replace("\r", "").Replace("\n", " || ");
        if (ex != null)
        {
            string cleanEx = ex.Message.Replace("\r", "").Replace("\n", " ");
            cleanMessage = $"{cleanMessage} | EX: {cleanEx}";
        }

        lock (_lock)
        {
            string payload = $"{timestamp}{level}{_context}{cleanMessage}"; 
            string currentHash = _crypto.ComputeSignature(_lastHash, payload);
 
            var logEntry = $"[{timestamp}] [{level,-5}] [{_context}] [{currentHash}] {cleanMessage}";

            System.Diagnostics.Debug.WriteLine(logEntry);

            try 
            { 
                File.AppendAllText(LogFilePath, logEntry + Environment.NewLine);
                _lastHash = currentHash; 
            }
            catch 
            {
                // Fallback: If disk write fails, we might crash or write to fallback, 
                // but we shouldn't update _lastHash if the write failed.
            }
        }
    }
}