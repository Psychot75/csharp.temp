using System.Text.RegularExpressions;

namespace Arcraven.Avalonia.ResourcesLib.Logging;

public class LogValidator
{
    /// <summary>
    /// Verifies the cryptographic integrity of the log chain using HMAC signatures.
    /// </summary>
    public bool VerifyLogFile(string logPath, string seedPath, out int errorLine)
    {
        errorLine = -1;

        if (!File.Exists(logPath)) return true; // No logs = valid

        if (!File.Exists(seedPath)) return false; 

        LogCryptoProvider crypto;
        try
        {
            crypto = new LogCryptoProvider();
        }
        catch
        {
            return false;
        }

        string lastHash = crypto.GetGenesisHash();
        int currentLineIndex = 0;

        var logRegex = new Regex(@"^\[(?<ts>.*?)\] \[(?<lvl>.*?)\] \[(?<ctx>.*?)\] \[(?<hash>.*?)\] (?<msg>.*)$");

        foreach (var line in File.ReadLines(logPath))
        {
            currentLineIndex++;
            var match = logRegex.Match(line);

            if (!match.Success) continue; // Skip malformed lines

            string ts = match.Groups["ts"].Value;
            
            string lvl = match.Groups["lvl"].Value.Trim(); 
            string ctx = match.Groups["ctx"].Value;
            string recordedHash = match.Groups["hash"].Value;
            string msg = match.Groups["msg"].Value;
            
            string payload = $"{ts}{lvl}{ctx}{msg}";

            string calculatedHash = crypto.ComputeSignature(lastHash, payload);

            if (recordedHash != calculatedHash)
            {
                errorLine = currentLineIndex;
                return false;
            }
            
            lastHash = calculatedHash;
        }

        return true;
    }
}