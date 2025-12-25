using System;
using System.Reflection;
using Avalonia.Platform;

namespace Arcraven.Avalonia.Viewers.Controls;

internal sealed class WebviewGtkBackend : IWebViewBackend
{
    private static readonly string[] TypeNames =
    {
        "WebviewGtk.WebView, WebviewGtk",
        "WebviewGtk.Webview, WebviewGtk"
    };

    private readonly object _instance;
    private readonly MethodInfo? _navigateMethod;
    private readonly IPlatformHandle _handle;

    private WebviewGtkBackend(object instance, MethodInfo? navigateMethod, IPlatformHandle handle)
    {
        _instance = instance;
        _navigateMethod = navigateMethod;
        _handle = handle;
    }

    public IPlatformHandle Handle => _handle;

    public static IWebViewBackend? TryCreate(IPlatformHandle parent)
    {
        var type = ResolveType();
        if (type == null)
        {
            return null;
        }

        object? instance = Activator.CreateInstance(type);
        if (instance == null)
        {
            return null;
        }

        TryAttachToParent(type, instance, parent);

        var handle = GetHandle(type, instance);
        if (handle == IntPtr.Zero)
        {
            return null;
        }

        var platformHandle = new PlatformHandle(handle, OperatingSystem.IsWindows() ? "HWND" : "GtkWidget");
        var navigateMethod = FindNavigateMethod(type);

        return new WebviewGtkBackend(instance, navigateMethod, platformHandle);
    }

    public void Navigate(Uri? source)
    {
        if (source == null || _navigateMethod == null)
        {
            return;
        }

        _navigateMethod.Invoke(_instance, new object[] { source.ToString() });
    }

    public void Dispose()
    {
        if (_instance is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }

    private static Type? ResolveType()
    {
        foreach (var typeName in TypeNames)
        {
            var type = Type.GetType(typeName, throwOnError: false);
            if (type != null)
            {
                return type;
            }
        }

        return null;
    }

    private static MethodInfo? FindNavigateMethod(Type type)
    {
        return type.GetMethod("Navigate", new[] { typeof(string) })
            ?? type.GetMethod("LoadUri", new[] { typeof(string) })
            ?? type.GetMethod("LoadUrl", new[] { typeof(string) })
            ?? type.GetMethod("LoadURL", new[] { typeof(string) })
            ?? type.GetMethod("Load", new[] { typeof(string) });
    }

    private static IntPtr GetHandle(Type type, object instance)
    {
        var handleProperty = type.GetProperty("Handle") ?? type.GetProperty("NativeHandle");
        if (handleProperty?.GetValue(instance) is IntPtr handle)
        {
            return handle;
        }

        return IntPtr.Zero;
    }

    private static void TryAttachToParent(Type type, object instance, IPlatformHandle parent)
    {
        var method = type.GetMethod("SetParent", new[] { typeof(IntPtr) })
            ?? type.GetMethod("AttachToParent", new[] { typeof(IntPtr) })
            ?? type.GetMethod("Attach", new[] { typeof(IntPtr) })
            ?? type.GetMethod("SetHost", new[] { typeof(IntPtr) });

        method?.Invoke(instance, new object[] { parent.Handle });
    }
}