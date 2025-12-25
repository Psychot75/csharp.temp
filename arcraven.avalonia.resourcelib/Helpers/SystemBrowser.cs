using System.Diagnostics;
using System.Net;
using System.Runtime.InteropServices;
using Duende.IdentityModel.OidcClient.Browser;

namespace Arcraven.Avalonia.ResourcesLib.Helpers;

/// <summary>
/// A modular helper that implements the IBrowser interface for desktop environments.
/// </summary>
public class SystemBrowser : IBrowser
{
    private readonly string _callbackUrl;

    public SystemBrowser(string callbackUrl = "http://localhost:5000/callback/")
    {
        _callbackUrl = callbackUrl;
    }

    public async Task<BrowserResult> InvokeAsync(BrowserOptions options, CancellationToken cancellationToken = default)
    {
        using var listener = new HttpListener();
        listener.Prefixes.Add(_callbackUrl);
        listener.Start();

        // Use a helper method to handle cross-platform process starting
        BrowserHelper.OpenUrl(options.StartUrl);

        var context = await listener.GetContextAsync();
        
        // Respond to the user in the browser
        await SendResponseAsync(context.Response);

        return new BrowserResult
        {
            Response = context.Request.Url?.ToString(),
            ResultType = BrowserResultType.Success
        };
    }

    private async Task SendResponseAsync(HttpListenerResponse response)
    {
        string html = "<html><body><h1>Login Successful</h1><p>You can return to the HMI now.</p></body></html>";
        var buffer = System.Text.Encoding.UTF8.GetBytes(html);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.Close();
    }
}

/// <summary>
/// Static helper to handle the nuances of opening a URL on different Operating Systems.
/// </summary>
public static class BrowserHelper
{
    public static void OpenUrl(string url)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            Process.Start(new ProcessStartInfo(url.Replace("&", "^&")) { UseShellExecute = true });
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            Process.Start("xdg-open", url);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            Process.Start("open", url);
        }
    }
}