using System;
using Avalonia.Platform;

namespace Arcraven.Avalonia.Viewers.Controls;

internal static class WebViewBackendFactory
{
    public static IWebViewBackend? Create(IPlatformHandle parent)
    {
        if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux())
        {
            return WebviewGtkBackend.TryCreate(parent);
        }

        return null;
    }
}