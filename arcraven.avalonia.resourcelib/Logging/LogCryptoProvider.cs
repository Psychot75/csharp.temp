using System.Security.Cryptography;
using System.Text;
using Arcraven.Avalonia.ResourcesLib.Helpers;

namespace Arcraven.Avalonia.ResourcesLib.Logging;

public class LogCryptoProvider
{
    private readonly byte[] _secretKey;

    public LogCryptoProvider()
    {
        _secretKey = LoadOrGenerateKey();
    }

    /// <summary>
    /// Loads the HMAC secret key from the SecureStorage seed path.
    /// If it doesn't exist, it generates a new 256-bit cryptographically strong key.
    /// </summary>
    private byte[] LoadOrGenerateKey()
    {
        var path = SecureStorage.SeedPath;

        try 
        {
            if (File.Exists(path))
            {
                var hexKey = File.ReadAllText(path).Trim();
                return Convert.FromHexString(hexKey);
            }
        }
        catch 
        { 
            // TODO
            // Fallback if corrupted/empty
        }
        
        var newKey = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(newKey);
        }

        SecureStorage.InitializeSeed();
        
        try
        {
            File.WriteAllText(path, Convert.ToHexString(newKey));
            
            // Re-apply strict permissions on Linux specifically for this file
            if (System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Linux))
            {
                // TODO
                // Simple chmod 600 logic (read/write owner only)
                // You can reuse your RunChmod helper here if accessible, or rely on SecureStorage
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[CRITICAL] Could not save Audit Key: {ex.Message}");
        }

        return newKey;
    }

    /// <summary>
    /// Computes an HMAC signature for a log entry.
    /// Formula: HMAC_SHA256(Key, PreviousHash + ContentPayload)
    /// </summary>
    public string ComputeSignature(string previousHash, string contentPayload)
    {
        using var hmac = new HMACSHA256(_secretKey);
        
        // We bind the Previous Hash to the Current Content.
        // This ensures the chain order cannot be altered.
        var dataToSign = Encoding.UTF8.GetBytes(previousHash + contentPayload);
        
        var signatureBytes = hmac.ComputeHash(dataToSign);
        
        // Return as hex string (truncated to 16 chars if you want short logs, 
        // or full length for max security. Using 16 chars matches your previous format).
        return Convert.ToHexString(signatureBytes)[..16];
    }

    /// <summary>
    /// Returns the "Genesis Hash" for a brand new log file based on the secret key.
    /// </summary>
    public string GetGenesisHash()
    {
        using var sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(_secretKey))[..16];
    }
}